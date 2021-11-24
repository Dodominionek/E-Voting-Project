from Server import ConsoleApp
from os import getenv
from sys import argv

if __name__ == '__main__':
    print('Running')
    port = getenv('PORT')

    if port is None:
        if len(argv) == 2:
            port = int(argv[1])
        elif len(argv) <= 1:
            port = 9999
        else:
            print('Missing argument or too many')

    if port is not None:
        app = ConsoleApp(port)