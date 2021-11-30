from flask import Flask, request, jsonify, make_response
from flask_sqlalchemy import SQLAlchemy
from flask_marshmallow import Marshmallow
from flask_restful import Resource, Api
from flask_jwt_extended import JWTManager, create_access_token, jwt_required, get_jwt
from datetime import timedelta, datetime
import pytz, time, threading, os

app = Flask(__name__) 
api = Api(app) 

app.config['SQLALCHEMY_DATABASE_URI'] = 'sqlite:///dodomin.db' 
app.config['SQLALCHEMY_TRACK_MODIFICATIONS'] = True 
app.config["JWT_SECRET_KEY"] = os.urandom(24).hex()
app.config["JWT_ACCESS_TOKEN_EXPIRES"] = timedelta(minutes=30)

db = SQLAlchemy(app) 
ma = Marshmallow(app)
jwt = JWTManager(app)
jwt.init_app(app)

####### User ##############
class User(db.Model): 
    user  = db.Table('user',
    db.Column('id', db.Integer, primary_key=True),
    db.Column('email', db.String(64)),
    db.Column('username', db.String(64), unique=True),
    db.Column('password', db.String(128)),
    sqlite_autoincrement=True)

    def __init__(self, username, password, email):
        self.username = username
        self.password = password
        self.email = email

class UserSchema(ma.Schema):
    class Meta:
        fields = ('id', 'username', 'password', 'email')
        session = db.Session

user_schema = UserSchema() 
users_schema = UserSchema(many=True)

####### Token BlackList ##############
class TokenBlackList(db.Model):
    id = db.Column(db.Integer, primary_key=True)
    jti = db.Column(db.String(36), nullable=False)
    expiration_time = db.Column(db.DateTime, default=datetime.utcnow, nullable=False)

class TokenBlacklistSchema(ma.Schema):
    class Meta:
        fields = ('id', 'jti', 'expiration_time')

token_blacklist_schema = TokenBlacklistSchema() 
tokens_blacklist_schema =  TokenBlacklistSchema(many=True)

####### Voting ##############
class Voting(db.Model): 
    voting  = db.Table('voting',
    db.Column('id', db.Integer, primary_key=True),
    db.Column('question', db.String(256)),
    db.Column('answerA', db.String(256)),
    db.Column('answerB', db.String(256)),
    db.Column('answerC', db.String(256)),
    db.Column('answerD', db.String(256)),
    db.Column('status', db.String(64)),
    sqlite_autoincrement=True)

    def __init__(self, question, answerA, answerB, answerC, answerD, status):
        self.question = question
        self.answerA = answerA
        self.answerB = answerB
        self.answerC = answerC
        self.answerD = answerD
        self.status = status

class VotingSchema(ma.Schema):
    class Meta:
        fields = ('id', 'question', 'answerA', 'answerB', 'answerC', 'answerD', 'status')
        session = db.Session

voting_schema = VotingSchema() 
votings_schema = VotingSchema(many=True)

####### Vote ##############
class Vote(db.Model): 
    vote  = db.Table('vote',
    db.Column('id', db.Integer, primary_key=True),
    db.Column('question', db.String(64)),
    db.Column('votingId', db.String(64)),
    db.Column('userAnswer', db.String(64)),
    sqlite_autoincrement=True)

    def __init__(self, username, votingId, userAnswer):
        self.username = username
        self.votingId = votingId
        self.userAnswer = userAnswer

class VoteSchema(ma.Schema):
    class Meta:
        fields = ('id', 'username', 'votingId', 'userAnswer')
        session = db.Session

vote_schema = VoteSchema() 
votes_schema = VoteSchema(many=True)
    
################### RESOURCES #######################

####### Register Manager ##############
class RegisterManager(Resource):
    @staticmethod
    def post():
        try:  
            username = request.json['username']
            password = request.json['password']
            email = request.json['email']
        except Exception as _:
            username = None
            password = None
            email = None

        if (username and password and email) != None and User.query.filter_by(username = username).first() == None:
            user = User(username, password, email)
            db.session.add(user)
            db.session.commit()
            return make_response(jsonify({'Message': f'User {username} inserted.'}), 201)
        elif User.query.filter_by(username = username).first() != None:
            return make_response(jsonify({'Message': f'User {username} exist.'}), 409)
        else:
            return make_response(jsonify({'Message': 'BAD_REQUEST'}), 400)

####### User Manager ##############
class UserManager(Resource):
    @staticmethod
    @jwt_required()
    def get():
        try: username = request.args['username']
        except Exception as _: username = None

        if not username:
            users = User.query.all()
            return make_response(jsonify(users_schema.dump(users)), 200)
        elif User.query.filter_by(username = username).first() != None:
            user = User.query.filter_by(username = username).first()
            return make_response(jsonify(user_schema.dump(user)), 200)
        else:
            return make_response(jsonify({'Message': 'NOT FOUND'}), 404)
                  
    @staticmethod
    @jwt_required()
    def put():
        try: username = request.args['username']
        except Exception as _: username = None

        try:  
            username_new = request.json['username']
            password_new = request.json['password']
            email_new = request.json['email']
        except Exception as _:
            username_new = None
            password_new = None
            email_new = None

        if not username or username_new == None or password_new == None or email_new == None:
            return make_response(jsonify({ 'Message': 'Must provide the proper data' }), 400)

        user = User.query.filter_by(username = username).first()

        if user == None:
            return make_response(jsonify({ 'Message': 'User not exist!' }), 404)

        user.password = password_new
        user.username = username_new
        user.email = email_new

        db.session.commit()
        return make_response(jsonify({'Message': f'User {user.username} altered.'}), 200)

    @staticmethod
    @jwt_required()    
    def patch():
        try:  username = request.args['username']
        except Exception as _: username = None
            
        try: username_new = request.json['username']
        except Exception as _: username_new = None

        try: password_new = request.json['password']
        except Exception as _: password_new = None

        try: email_new = request.json['email']
        except Exception as _: email_new = None

        if username == None:
            return make_response(jsonify({ 'Message': 'Must provide the proper username' }), 400)

        user = User.query.filter_by(username = username).first()

        if user == None:
            return make_response(jsonify({ 'Message': 'User not exist!' }), 404)

        if username_new != None:
            user.username = username_new

        if password_new != None:
            user.password = password_new 
        
        if email_new != None:
            user.email = email_new 

        db.session.commit()
        return make_response(jsonify({'Message': f'User {user.username} altered.'}), 200)

    @staticmethod
    @jwt_required()
    def delete():
        try: username = request.args['username']
        except Exception as _: username = None

        if not username:
            return make_response(jsonify({ 'Message': 'Must provide the user username' }), 400)

        user = User.query.filter_by(username = username).first()

        if user == None:
            return make_response(jsonify({ 'Message': 'User not exist!' }), 404)
        
        db.session.delete(user)
        db.session.commit()

        return make_response(jsonify({'Message': f'User {username} deleted.'}), 200)

####### Login Manager ##############
class LoginManager(Resource):
    @staticmethod
    def post():
        try:
            username = request.json['username']
            password = request.json['password']
        except Exception as _:
            username = None
            password = None
        
        if None in [username, password]:
            return make_response(jsonify({'Message': 'BAD_REQUEST'}), 400)
        
        user = User.query.filter_by(username = username).first()

        if user is None:
            return make_response(jsonify({'Message': f'User {username} not found.'}), 404)
        elif user.password != password:
            return make_response(jsonify({'Message': 'Wrong password'}), 400)

        try:  
            username = request.json['username']
            password = request.json['password']
        except Exception as _:
            username = None
            password = None
        
        if None not in [username, password]:
            user =  User.query.filter_by(username = username).first()
            if user != None and user.password == password:
                access_token = create_access_token(identity=user.id)
                return make_response(jsonify({'token': f'{access_token}'}), 201)
            else:
                return make_response(jsonify({'Message': 'Wrong password'}), 400)
        else:
            return make_response(jsonify({'Message': 'BAD_REQUEST'}), 400)

####### LogoutManager ##############
class LogoutManager(Resource):
    @staticmethod
    @jwt_required()
    def post():
        jti = get_jwt()["jti"]
        exp_time = datetime.fromtimestamp(get_jwt()["exp"], pytz.utc)
        db.session.add(TokenBlackList(jti=jti, expiration_time=exp_time))
        db.session.commit()
        return make_response(jsonify({ 'Message': 'Succesfully logout' }), 201)

####### Voting Manager ##############
class VotingManager(Resource):
    @staticmethod
    def get():
        try: votingId = request.json['votingId']
        except Exception as _: votingId = None

        if not votingId:
            votings = Voting.query.all()
            return make_response(jsonify(votings_schema.dump(votings)), 200)
        elif Voting.query.filter_by(votingId = votingId).first() != None:
            voting = Voting.query.filter_by(votingId = votingId).first()
            return make_response(jsonify(voting_schema.dump(voting)), 200)
        else:
            return make_response(jsonify({'Message': 'NOT FOUND'}), 404)
                  
    @staticmethod
    def post():
        try:
            username = request.json['username']
            question = request.json['question']
            answerA = request.json['answerA']
            answerB = request.json['answerB']
            answerC = request.json['answerC']
            answerD = request.json['answerD']
        except Exception as _: 
            username = None
            question = None
            answerA = None
            answerB = None
            answerC = None
            answerD = None

        if not username or not question or not answerA or not answerB:
            return make_response(jsonify({ 'Message': 'Must provide the proper data' }), 400)

        user = User.query.filter_by(username = username).first()

        if user == None:
            return make_response(jsonify({ 'Message': 'User not exist!' }), 404)

        voting = Voting(question, answerA, answerB, answerC, answerD, 'Created')

        db.session.commit()
        return make_response(jsonify({'Message': f'New Voting {voting.id} created.'}), 201)

    @staticmethod
    def delete():
        try: 
            username = request.json['username']
            votingId = request.json['votingId']
        except Exception as _: 
            username = None
            votingId = None

        if not username or not votingId:
            return make_response(jsonify({ 'Message': 'No such voting.' }), 400)

        voting = Voting.query.filter_by(votingId = votingId).first()

        if voting == None:
            return make_response(jsonify({ 'Message': 'No such voting.' }), 404)
        
        db.session.delete(voting)
        db.session.commit()

        return make_response(jsonify({'Message': f'Voting {votingId} deleted.'}), 200)

####### VoteManager ##############
class VoteManager(Resource):
    @staticmethod
    def post():
        try:
            username = request.json['username']
            votingId = request.json['votingId']
            userAnswer = request.json['userAnswer']
        except Exception as _:
            username = None
            votingId = None
            userAnswer = None
        
        if None in [username, votingId, userAnswer]:
            return make_response(jsonify({'Message': 'BAD_REQUEST'}), 400)
        
        voting = Voting.query.filter_by(votingId = votingId).first()

        if voting is None:
            return make_response(jsonify({'Message': f'Voting {votingId} not found.'}), 404)

        try:
            username = request.json['username']
            votingId = request.json['votingId']
            userAnswer = request.json['userAnswer']
        except Exception as _:
            username = None
            votingId = None
            userAnswer = None
        
        if None not in [username, votingId, userAnswer]:
            voting =  Voting.query.filter_by(votingId = votingId).first()
            if voting != None and userAnswer != None and username != None:
                vote = Vote(username, votingId, userAnswer)
                db.session.add(vote)
                db.session.commit()
                return make_response(jsonify({'Vote registered.'}), 201)
            else:
                return make_response(jsonify({'Message': 'Vote was not registered.'}), 400)
        else:
            return make_response(jsonify({'Message': 'BAD_REQUEST'}), 400)



####### Functions ##############

@jwt.token_in_blocklist_loader
def check_if_token_revoked(jwt_header, jwt_payload):
    jti = jwt_payload["jti"]
    token = db.session.query(TokenBlackList.id).filter_by(jti=jti).scalar()
    return token is not None

def delete_expired_tokens():
    jwt_tokens = TokenBlackList.query.all()
    jwt_tokens_list = tokens_blacklist_schema.dump(jwt_tokens)
    now_str = datetime.utcnow().isoformat(' ', 'seconds')
    now = datetime.strptime(now_str, '%Y-%m-%d %H:%M:%S')
    for jwt_token in jwt_tokens_list:
        if now > datetime.strptime(jwt_token['expiration_time'], '%Y-%m-%dT%H:%M:%S'):
            TokenBlackList.query.filter_by(id=jwt_token['id']).delete()
            db.session.commit()

def schedule_tokens():
    while(True):
        # print(threading.active_count())
        with app.app_context():
            delete_expired_tokens()
        time.sleep(1)

####### Resource Mapping ##############

api.add_resource(RegisterManager, '/signup')
api.add_resource(UserManager, '/user')
api.add_resource(LoginManager, '/login')
api.add_resource(LogoutManager, '/logout')
api.add_resource(VotingManager, '/voting')
api.add_resource(VoteManager, '/vote')

if __name__ == '__main__':
    try:
        t2 = threading.Thread(target=schedule_tokens)
        t2.daemon = True
        t2.start()
       
        app.run(debug=True)
    except Exception as ex:
        t2.join
        print(ex)
