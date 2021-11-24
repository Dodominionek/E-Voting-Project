import socket
import threading
import time
import datetime
import pytz
import sys
import os
import re

class ConnectedUser(object):
    current_ids = set()

    def __init__(self, socket, address):
        self.socket = socket
        self.address = address
        self.connected = True
        self.name = None
        
        self.id = None

        self.UDP = None

        for i in range(1000):
            if not ConnectedUser.current_ids.issuperset(set([i])):
                self.id = i
                ConnectedUser.current_ids.add(i)
                break
    
    def disconnected(self):
        self.connected = False
        ConnectedUser.current_ids.remove(self.id)

    def __repr__(self):
        return 'ID='+str(self.id)+' ADDR='+str(self.address)+' NAME='+str(self.name)+' STATUS='+str(self.connected)+ ' UDP='+str(self.UDP)

class Server(object):
    def __init__(self,port: int):
        self.ip = "0.0.0.0"
        self.port = port
        self.running = True

        self.timezone = pytz.timezone('Europe/Warsaw')

        try:
            self.sock = socket.socket(socket.AF_INET,socket.SOCK_STREAM)
            self.sock.bind((self.ip,self.port))

            self.udp = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
            self.udp.bind((self.ip,self.port))
            self.udpThread = threading.Thread(target=self.handleUDP)
        except:
            print('Something went wrong on port bind')
        
        self.connections = []

    def run(self):
        print('Running on: ' + str(self.ip) + ':' + str(self.port) )
        
        self.sock.listen(50)

        self.udpThread.start()

        while self.running:
            print('Awaiting more connections...')
            try: 
                connection, address = self.sock.accept()

                print(str(address) + ' connected')

                user = ConnectedUser(connection,address)

                self.connections.append(user)
                threading.Thread(target=self.handleTCP,args=(user,)).start()
            except WindowsError as err:
                pass
            except Exception as err:
                print('Some error occured ' + str(err))
                pass

    def handleTCP(self, user: ConnectedUser):

        while self.running and user.connected:
            try:
                recv = user.socket.recv(1024)
                p = re.compile(r'(?<=\})(?=\{)')
                slices = re.split(p, recv.decode())

                for x in slices:
                    print(x)
            except socket.error:
                user.socket.close()
                user.disconnected()
                print(str(user.address) + ' disconnected')
                self.connections.remove(user)
                
                self.sendAll('Send all test')
    
    def handleUDP(self):

        while self.running:
            try:
                recv = self.udp.recvfrom(1024)
                if self.validReciever(recv[1]):
                    self.sendBroadcastUDP(recv[0],recv[1])
            except socket.error as ex:
                pass

    def send(self,connection,data):
        try:
            connection.send(data)
        except:
            pass

    def sendAll(self,data):
        for user in self.connections:
            try:
                user.socket.send(data)
            except:
                pass
    
class ConsoleApp(object):
    def __init__(self, port: int):
        self.port = port

        self.server = Server(self.port)

        self.thread = threading.Thread(target=self.server.run)      

        print('Starting server')
        self.thread.start()
        
        self.run()

    def run(self):
        while(True):
            val = str(input()).upper()
            if val == 'STOP':
                print('Stopping server...')
                self.server.stop()
                self.thread.join()
                break
            elif val == 'LIST':
                print('Getting current user list')
                for user in self.server.connections:
                    print(user) 