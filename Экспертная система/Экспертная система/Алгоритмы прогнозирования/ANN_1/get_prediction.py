prediction_algorithm_name = 'ANN_1'
print("СКРИПТ ПОТОЧНОГО ПРОГНОЗИРОВАНИЯ " + prediction_algorithm_name + " ЗАПУЩЕН...") 

import random
random.seed()
session=random.getrandbits(16)
print("session = "+(str)(session))

import time
import sys
import argparse
import numpy
from pandas import read_csv
from datetime import datetime
from math import sqrt
from numpy import concatenate
from matplotlib import pyplot
from pandas import DataFrame
from pandas import concat
from keras.models import Sequential
from keras.layers import Dense
from keras.layers import LSTM
from keras.models import load_model
import json
print(sys.platform)
def createParser():
    parser = argparse.ArgumentParser()
    #parser.add_argument('--json_file_path',type=str,default='D:\Anton\Desktop\MAIN\Экспертная система\Экспертная система\Алгоритмы прогнозирования\LSTM 1\json.txt')
    parser.add_argument('--json_file_path',type=str,default='C:\\Users\\anton\\Рабочий стол\\MAIN\\Экспертная система\\Экспертная система\\Алгоритмы прогнозирования\\LSTM 1\\json.txt')
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
AVG=(float)(h("AVG"))
print("AVG: "+(str)(AVG))
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
                #print(line)
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

        print("dataset = ",dataset)
        X = numpy.zeros((1,window_size), dtype=float)
        predicted_column_index = (int)(h("predicted_column_index"))
        for j in range(0,window_size):
              X[0,j] = dataset[j+(len(lines)-window_size)][predicted_column_index]
        print("X = ",X)
        print("X shape: ",X.shape)

        predicted = model.predict(X)
        print(predicted)
        predicted=predicted-AVG+0.5
        print("AVG: "+(str)(AVG))
        print("session = "+(str)(session))
        print(predicted)
        print("prediction:",predicted)