prediction_algorithm_name = 'LSTM_2'
print("СКРИПТ ПОТОЧНОГО ПРОГНОЗИРОВАНИЯ " + prediction_algorithm_name + " ЗАПУЩЕН...")

import random
random.seed()
session=random.getrandbits(16)
print("session = "+(str)(session))


from cntk.device import try_set_default_device, gpu
import cntk.device as C
print(C.all_devices())
print(C.try_set_default_device(C.gpu(0)))
print(C.use_default_device())  
import time
import sys
import argparse
import numpy
from datetime import datetime
from keras.models import Sequential
from keras.layers import Dense
from keras.layers import LSTM
from keras.models import load_model
import json

#print(sys.platform)
def createParser():
    parser = argparse.ArgumentParser()
    #parser.add_argument('--json_file_path',type=str,default='D:\Anton\Desktop\MAIN\Экспертная система\Экспертная система\Алгоритмы прогнозирования\LSTM 1\h.json')
    parser.add_argument('--json_file_path',type=str)
    return parser

parser = createParser()
args = parser.parse_args()
jsonFile = open(args.json_file_path, 'r')
jsontext = jsonFile.read()
jsonFile.close()
jsonObj = json.loads(jsontext)
baseNodeName=  next((v for i, v in enumerate(jsonObj.items()) if i == 0))[0]

def h(nodeName):
    return  jsonObj[baseNodeName][nodeName]["value"]

def h2(nodeName1,nodeName2):
    return  jsonObj[baseNodeName][nodeName1][nodeName2]["value"]

def getAttr2(nodeName1,nodeName2,attrName):
    return  jsonObj[baseNodeName][nodeName1][nodeName2][attrName]

def getAttr2int(nodeName1,nodeName2,attrName):
    return  (int)(jsonObj[baseNodeName][nodeName1][nodeName2][attrName])



parser = createParser()
namespace = parser.parse_args()
print (namespace)

save_path = h("save_folder")+ 'weights.h5'  
try:
    model =load_model(save_path)
except:
    model =load_model(save_path.encode('ansi'))
window_size=(int)(h("window_size"))
print('model loaded')
print("save_path: "+save_path)
print("window_size: "+(str)(window_size)) 

enough=False
while enough==False: 
        is_end=False
        lines=[]
        #чтение из потока
        i=0
        while is_end==False:
            line= input()
            if line != 'over':
                lines.append(line)
                print(line)
                print('next')
                i=i+1
            else:
                is_end=True
       # print(" конец чтения потока")    


        dataset = numpy.zeros((len(lines), len(lines[0].split(';'))),dtype=float)
        for i in range(0,len(lines)):
            for j in range(0,len(lines[i].split(';'))):   
                featureStringValue = lines[i].split(';')[j]
                if featureStringValue != '\n':     
                    dataset[i ,j] = (float)(lines[i].split(';')[j])


       # print(dataset)
       # print(dataset.shape)
        X = numpy.zeros((1, window_size,dataset.shape[1]), dtype=float)
        predicted_column_index = (int)(h("predicted_column_index"))
        for j in range(0,window_size):
            for k in range(0,dataset.shape[1]):
                    X[0,j,k] = dataset[j][k]


        predicted = model.predict(X)

        if (predicted[0,2]>predicted[0,1])&(predicted[0,2]>predicted[0,0]):
            print("prediction:1")
        if (predicted[0,1]>predicted[0,2])&(predicted[0,1]>predicted[0,0]):
            print("prediction:0")
        if (predicted[0,0]>predicted[0,1])&(predicted[0,0]>predicted[0,2]):
            print("prediction:0.5")