from flask import Flask, request, jsonify, make_response
from flask_sqlalchemy import SQLAlchemy
from flask_marshmallow import Marshmallow
from flask_restful import Resource, Api
from flask_jwt_extended import JWTManager, create_access_token, jwt_required, get_jwt, get_jwt_identity
from datetime import timedelta, datetime
import pytz, time, threading, os

app = Flask(__name__) 
api = Api(app) 

app.config['SQLALCHEMY_DATABASE_URI'] = 'sqlite:///evoting.db'
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
    db.Column('ownerId', db.Integer),
    db.Column('question', db.String(256)),
    db.Column('answerA', db.String(256)),
    db.Column('answerB', db.String(256)),
    db.Column('answerC', db.String(256)),
    db.Column('answerD', db.String(256)),
    db.Column('status', db.String(64)),
    db.Column('timeStart', db.String(64)),
    db.Column('timeEnd', db.String(64)),
    sqlite_autoincrement=True)

    def __init__(self, question, ownerId, answerA, answerB, answerC, answerD, status, timeStart, timeEnd):
        self.question = question
        self.ownerId = ownerId
        self.answerA = answerA
        self.answerB = answerB
        self.answerC = answerC
        self.answerD = answerD
        self.status = status
        self.timeStart = timeStart
        self.timeEnd = timeEnd

class VotingSchema(ma.Schema):
    class Meta:
        fields = ('id', 'ovnerId' ,'question', 'answerA', 'answerB', 'answerC', 'answerD', 'status', 'timeStart', 'timeEnd')
        session = db.Session

voting_schema = VotingSchema() 
votings_schema = VotingSchema(many=True)

####### Result ##############
class Result(db.Model): 
    voting  = db.Table('result',
    db.Column('votingId', db.Integer, primary_key=True),
    db.Column('answerA', db.Integer),
    db.Column('answerB', db.Integer),
    db.Column('answerC', db.Integer),
    db.Column('answerD', db.Integer))

    def __init__(self, votingId, answerA, answerB, answerC, answerD):
        self.votingId = votingId
        self.answerA = answerA
        self.answerB = answerB
        self.answerC = answerC
        self.answerD = answerD

class ResultSchema(ma.Schema):
    class Meta:
        fields = ('votingId', 'answerA', 'answerB', 'answerC', 'answerD')
        session = db.Session

result_schema = ResultSchema() 
results_schema = ResultSchema(many=True)

####### Vote ##############
class Vote(db.Model): 
    vote  = db.Table('vote',
    db.Column('id', db.Integer, primary_key=True),
    db.Column('votingId', db.Integer),
    db.Column('userAnswer', db.CHAR),
    sqlite_autoincrement=True)

    def __init__(self, votingId, userAnswer):
        self.votingId = votingId
        self.userAnswer = userAnswer

class VoteSchema(ma.Schema):
    class Meta:
        fields = ('id', 'votingId', 'userAnswer')
        session = db.Session

vote_schema = VoteSchema() 
votes_schema = VoteSchema(many=True)

####### UserVote ##############
class UserVote(db.Model): 
    user_vote  = db.Table('user_vote',
    db.Column('id', db.Integer, primary_key=True),
    db.Column('votingId', db.Integer),
    db.Column('userId', db.Integer),
    sqlite_autoincrement=True)

    def __init__(self, votingId, userId):
        self.votingId = votingId
        self.userId = userId

class UserVoteSchema(ma.Schema):
    class Meta:
        fields = ('id', 'votingId', 'userId')
        session = db.Session

user_vote_schema = UserSchema() 
users_votes_schema = UserSchema(many=True)

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
    @jwt_required()
    def get():
        try: votingId = request.args['votingId']
        except Exception as _: votingId = None
        try: showUnvoted = request.args['showUnvoted']
        except Exception as _: showUnvoted = False
        try: onlyQuestions = request.args['onlyQuestions']
        except Exception as _: onlyQuestions = False
        
        if not votingId:
            if showUnvoted:
                user = User.query.filter_by(id = get_jwt_identity()).first()

                if onlyQuestions:    
                    voted = Voting.query.with_entities(Voting.id, Voting.question) \
                                        .join(UserVote, Voting.id == UserVote.votingId) \
                                        .filter_by(userId = user.id).all()
                    all_votings = Voting.query.with_entities(Voting.id, Voting.question).filter_by(status = 'Created').all()
                else:
                    voted = Voting.query.with_entities(Voting.id, Voting.question) \
                                        .join(UserVote, Voting.id == UserVote.votingId) \
                                        .filter_by(userId = user.id).all()
                    all_votings = Voting.query.with_entities(Voting.id, Voting.question, Voting.answerA, Voting.answerB, Voting.answerC, Voting.answerD, Voting.status, Voting.timeStart, Voting.timeEnd) \
                                        .filter_by(status = 'Created').all()

                for voting1 in voted:
                    for voting2 in all_votings[:]:
                        if voting1.id == voting2.id:
                            all_votings.remove(voting2)
                            break

                return make_response(jsonify(votings_schema.dump(all_votings)), 200)

            #show all voting ids and question
            if onlyQuestions:
                votings = Voting.query.with_entities(Voting.id, Voting.question).all()
            else:
                votings = Voting.query.with_entities(Voting.id, Voting.question, Voting.answerA, Voting.answerB, Voting.answerC, Voting.answerD, Voting.status, Voting.timeStart, Voting.timeEnd).all()
            return make_response(jsonify(votings_schema.dump(votings)), 200)

        #votingId provided
        voting = Voting.query.filter_by(id = votingId).first()
        if voting != None:
            return make_response(jsonify(voting_schema.dump(voting)), 200)
        else:
            return make_response(jsonify({'Message': 'Voting does not exist'}), 404)

    @staticmethod
    @jwt_required()
    def patch():
        try:
            endVoting = request.json['endVoting']
        except Exception as _:
            endVoting = False
        try:
            votingId = request.json['id']
        except Exception as _:
            votingId = None

        if not votingId:
            return make_response(jsonify({ 'Message': 'Missing parameter' }), 400)

        voting = Voting.query.filter_by(id = votingId, ownerId = get_jwt_identity()).first()

        if voting == None:
            return make_response(jsonify({ 'Message': 'No such voting.' }), 404)

        #ends voting on user demand
        if endVoting:
            voting.status = 'Ended'
            answerA = Vote.query.filter_by(votingId = votingId, userAnswer = 'A').count()
            answerB = Vote.query.filter_by(votingId = votingId, userAnswer = 'B').count()
            answerC = Vote.query.filter_by(votingId = votingId, userAnswer = 'C').count()
            answerD = Vote.query.filter_by(votingId = votingId, userAnswer = 'D').count()

            result = Result(votingId, answerA, answerB, answerC, answerD)

            #find all votes from this voting and remove them
            Vote.query.filter_by(votingId = votingId).delete()

            #find all entires for user votes for this voting and remove them
            UserVote.query.filter_by(votingId = votingId).delete()

            db.session.add(result)
            db.session.commit()

            return make_response(jsonify({ 'Message': 'Voting ended.' }), 200)
        
        return make_response(jsonify({ 'Message': 'No action performed.' }), 400)

    @staticmethod
    @jwt_required()
    def post():
        try:
            question = request.json['question']
            answerA = request.json['answerA']
            answerB = request.json['answerB']
            answerC = request.json['answerC']
            answerD = request.json['answerD']
            timeStart = request.json['timeStart']
            timeEnd = request.json['timeEnd']
        except Exception as _: 
            question = None
            answerA = None
            answerB = None
            answerC = None
            answerD = None
            timeStart = None
            timeEnd = None

        if not question or not answerA or not answerB:
            return make_response(jsonify({ 'Message': 'Must provide the proper data' }), 400)
        
        user = User.query.filter_by(id = get_jwt_identity()).first()

        if user == None:
            return make_response(jsonify({ 'Message': 'User not exist!' }), 404)

        voting = Voting(question, user.id, answerA, answerB, answerC, answerD, 'Created', timeStart, timeEnd)

        db.session.add(voting)
        db.session.commit()
        return make_response(jsonify({'Message': f'New Voting {question} created.', 'id': voting.id}), 201)

    @staticmethod
    @jwt_required()
    def delete():
        try: 
            votingId = request.json['votingId']
        except Exception as _:
            votingId = None

        if not votingId:
            return make_response(jsonify({ 'Message': 'Missing parameter' }), 400)

        voting = Voting.query.filter_by(id = votingId, ownerId = get_jwt_identity()).first()

        if voting == None:
            return make_response(jsonify({ 'Message': 'No such voting.' }), 404)

        #find all votes from this voting and remove them
        Vote.query.filter_by(votingId = votingId).delete()

        #find all entires for user votes for this voting and remove them
        UserVote.query.filter_by(votingId = votingId).delete()

        #find and remove result if voting ended
        Result.query.filter_by(votingId = votingId).delete()
        
        db.session.delete(voting)
        db.session.commit()

        return make_response(jsonify({'Message': f'Voting {votingId} deleted.'}), 200)

####### Result Manager ##############
class ResultManager(Resource):
    @staticmethod
    @jwt_required()
    def get():
        try: votingId = request.args['votingId']
        except Exception as _: votingId = None
        try: showVoted = request.args['showVoted']
        except Exception as _: showVoted = False
        
        if not votingId:
            if showVoted:
                user = User.query.filter_by(id = get_jwt_identity()).first()

                voted = Result.query.join(UserVote, Result.votingId == UserVote.votingId) \
                                    .filter_by(userId = user.id).all()

                return make_response(jsonify(results_schema.dump(voted)), 200)

            #show all
            results = Result.query.all()
            return make_response(jsonify(results_schema.dump(results)), 200)

        #votingId provided
        result = Result.query.filter_by(votingId = votingId).first()
        if result != None:
            return make_response(jsonify(result_schema.dump(result)), 200)
        else:
            return make_response(jsonify({'Message': 'Result for voting does not exist'}), 404)

####### VoteManager ##############
class VoteManager(Resource):
    @staticmethod
    @jwt_required()
    def post():
        voteTime = datetime.now()

        try:
            votingId = request.json['votingId']
            userAnswer = request.json['userAnswer']
        except Exception as _:
            votingId = None
            userAnswer = None
        
        if None not in [votingId, userAnswer]:
            voting =  Voting.query.filter_by(id = votingId).first()
            if voting is None:
                return make_response(jsonify({'Message': f'Voting {votingId} not found.'}), 404)

            if voting.status == 'Ended':
                return make_response(jsonify({'Message': f'Voting {votingId} has ended. No more votes can be casted'}), 403)

            try:
                votingStart = datetime.strptime(voting.timeStart, '%Y-%m-%d %H:%M:%S.%f')
                votingEnd = datetime.strptime(voting.timeEnd, '%Y-%m-%d %H:%M:%S.%f')
            except Exception as _:
                votingStart = None
                votingEnd = None

            print(voteTime)
            print(votingStart)
            print(votingEnd)
            

            if voting != None and userAnswer != None:
                #check if user already voted
                user = User.query.filter_by(id = get_jwt_identity()).first()
                user_vote = UserVote.query.filter_by(votingId = votingId, userId = user.id).first()
                if user_vote != None:
                    return make_response(jsonify({'Message': 'User already voted in this voting.'}), 403)

                if votingStart != None:
                    if voteTime < votingStart and votingStart != "":
                        return make_response(jsonify({'Message': 'Voting has not started yet.'}), 403)
                if votingEnd != None:
                    if voteTime > votingEnd:
                        return make_response(jsonify({'Message': 'Voting time is already over.'}), 403)

                #TODO przy zakończeniu głosowania i podliczeniu głosów usunąć vote i userVote powiązane z tym głosowaniem
                vote = Vote(votingId, userAnswer)
                user_vote = UserVote(votingId ,user.id)
                db.session.add(vote)
                db.session.add(user_vote)
                db.session.commit()
                return make_response(jsonify({'Message': 'Vote registered.'}), 201)
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
    while True:
        # print(threading.active_count())
        with app.app_context():
            delete_expired_tokens()
        time.sleep(1)

def check_voting_times():
    while True:
        time = datetime.now()
        votings = Voting.query.with_entities(Voting.id, Voting.timeStart, Voting.timeEnd, Voting.status).all()
        for voting in votings:
            if voting.timeStart != '' and voting.timeEnd != '':
                votingStart = datetime.strptime(voting.timeStart, '%Y-%m-%d %H:%M:%S.%f')
                votingEnd = datetime.strptime(voting.timeEnd, '%Y-%m-%d %H:%M:%S.%f')
                votingId = voting.id
                newStatus = ''
                if votingStart < time and votingEnd > time:
                    newStatus = 'Active'
                elif votingEnd < time:
                    newStatus = 'Ended'
                if newStatus != '':
                    matchingVoting = Voting.query.filter_by(id = votingId).first()
                    matchingVoting.status = newStatus
                    db.session.commit()





####### Resource Mapping ##############

api.add_resource(RegisterManager, '/signup')
api.add_resource(UserManager, '/user')
api.add_resource(LoginManager, '/login')
api.add_resource(LogoutManager, '/logout')
api.add_resource(VotingManager, '/voting')
api.add_resource(VoteManager, '/vote')
api.add_resource(ResultManager, '/result')

if __name__ == '__main__':
    try:
        t2 = threading.Thread(target=schedule_tokens)
        t2.daemon = True
        t2.start()

        t3 = threading.Thread(target=check_voting_times)       
        t3.start()

        app.run(debug=True)
    except Exception as ex:
        t2.join()
        t3.join()
        print(ex)
