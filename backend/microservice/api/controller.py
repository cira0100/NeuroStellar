import flask
from flask import request, jsonify
import ml_socket
import ml_service
import tensorflow as tf
import pandas as pd

app = flask.Flask(__name__)
app.config["DEBUG"] = True
app.config["SERVER_NAME"] = "127.0.0.1:5543"
  
class train_callback(tf.keras.callbacks.Callback):
    def __init__(self, x_test, y_test):
        self.x_test = x_test
        self.y_test = y_test
    #
    def on_epoch_end(self, epoch, logs=None):
        print(epoch)
        #print('Evaluation: ', self.model.evaluate(self.x_test,self.y_test),"\n") #broj parametara zavisi od izabranih metrika loss je default

@app.route('/train', methods = ['POST'])
def train():
    print("******************************TRAIN*************************************************")
    f = request.json["dataset"]
    dataset = pd.read_csv(f)
    #
    result = ml_service.train(dataset, request.json["model"], train_callback)
    print(result)
    return jsonify(result)

@app.route('/predict', methods = ['POST'])
def predict():
    f = request.json['filepath']
    dataset = pd.read_csv(f)
    m = request.json['modelpath']
    #model = tf.keras.models.load_model(m)
    #
    #model.predict?

print("App loaded.")
ml_socket.start()
app.run()