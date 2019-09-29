prediction_algorithm_name = 'Easy'

import time
import sys
import argparse
import numpy
from datetime import datetime
from keras.models import load_model
import json

import argparse
def createParser():
    parser = argparse.ArgumentParser()
    parser.add_argument('--json_file_path',type=str,default='E:\Anton\Desktop\MAIN\Экспертная система\Экспертная система\Алгоритмы прогнозирования\Easy\h.json')
    return parser
parser = createParser()
args = parser.parse_args()
#####################################################################
# лог
import os
logPath = os.path.dirname(args.json_file_path) + "\\log.txt"
logErrorPath = os.path.dirname(args.json_file_path) + "\\log_error.txt"
import sys
try:
    sys.stderr = open(logErrorPath, 'w')
    logFile = open(logPath,"w")
    logFile.write(logPath)
    logFile.close()
except:
    print("не удалось открыть файл логгирования ",logPath)
def log(s):
    try:
        logFile = open(logPath,"a")
        logFile.write((str)(s) + '\n')
        logFile.close()
        print(s)
    except:
        print(s)
log(logPath)
######################################################################
log("ЦИКЛИЧНОЕ ПРОГНОЗИРОВАНИЕ " + prediction_algorithm_name + " ...") 


jsonFile = open(args.json_file_path, 'r')
jsontext = jsonFile.read()
jsonFile.close()
jsonObj = json.loads(jsontext)
baseNodeName = next((v for i, v in enumerate(jsonObj.items()) if i == 0))[0]

def h(request):
    nodeArray = request.split('/')
    if(len(nodeArray) == 1):
        return jsonObj[baseNodeName][nodeArray[0]]
    if(len(nodeArray) == 2):
        return jsonObj[baseNodeName][nodeArray[0]][nodeArray[1]]
    if(len(nodeArray) == 3):
        return jsonObj[baseNodeName][nodeArray[0]][nodeArray[1]][nodeArray[2]]
    if(len(nodeArray) == 4):
        return jsonObj[baseNodeName][nodeArray[0]][nodeArray[1]][nodeArray[2]][nodeArray[3]]
    if(len(nodeArray) == 5):
        return jsonObj[baseNodeName][nodeArray[0]][nodeArray[1]][nodeArray[2]][nodeArray[3]][nodeArray[4]]
    if(len(nodeArray) == 6):
        return jsonObj[baseNodeName][nodeArray[0]][nodeArray[1]][nodeArray[2]][nodeArray[3]][nodeArray[4]][nodeArray[5]]
    return  "PARAMETER NOT FOUND"


save_path = h("save_folder/value") + 'weights.h5'  
try:
    model = load_model(save_path)
except:
    model = load_model(save_path.encode('ansi'))
window_size = (int)(h("window_size/value"))
print('model loaded')
print("save_path: " + save_path)
print("window_size: " + (str)(window_size)) 

inputFile = open(h("input_file/value"))
#превращение входного файла в плоскую таблицу значений предикторов
lines = inputFile.readlines()

predicted_column_index = (int)(h("predicted_column_index/value"))

if (h("NN_struct/layer1/value") == "LSTM") | (h("NN_struct/layer1/value") == "Conv1D"):
    X = numpy.zeros((1, window_size,1), dtype=numpy.float32)
else:
    X = numpy.zeros((1, window_size), dtype=numpy.float32)

print("X.shape: ",X.shape)

cycles = 500

predictionsFile = open('cyclic_prediction.txt', 'w')
head = lines[0].split(';')[predicted_column_index]

#log(h("predictions_file_path"))
head = head = lines[0].split(';')[predicted_column_index].replace('\n','') + ';' + '(predicted -> )' + lines[0].split(';')[(int)(h("predicted_column_index/value"))].replace('\n','') + ';type'
# if head[:-1]==';':                            6
#     head = head[0:-1]
predictionsFile.write(head + '\n')

start_point=(int)(len(lines)*(float)(h('split_point/value')   ))

data=[]
for i in range(0,window_size):
    line = ''
    line = line + (str)(lines[start_point+i+1]).replace('\n','') + ';' + lines[start_point+i+1].split(';')[(int)(h("predicted_column_index/value"))].replace('\n','') + '; real (1st window)'
    predictionsFile.write(line+ '\n')
    data.append(lines[start_point+i+1])

for c in range(0,cycles):
    for i in range(0,window_size):
        

        if (h("NN_struct/layer1/value") == "LSTM") | (h("NN_struct/layer1/value") == "Conv1D"):
            for j in range(0,len(data[i].split(';'))):   
                featureStringValue = data[i].split(';')[j]
                if featureStringValue != '\n':     
                    X[0,i ,j] = (float)(data[i].split(';')[j])
        else:
             X[0,i] = (float)(data[i].split(';')[predicted_column_index])


    predicted = model.predict(X)
    line = ''
    line = line + (str)(lines[start_point+cycles + c+1]).replace('\n','') + ';'
    line = line + (str)(predicted[0,0]) + '; '
    line = line +  (str)(c)+' prediction'
    predictionsFile.write(line + '\n')



    data=numpy.delete(data, 0)
    data=numpy.append(data,(str)(predicted[0,0]))







predictionsFile.close()
