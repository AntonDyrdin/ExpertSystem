# ЦИКЛИЧНОЕ ПРОГНОЗИРОВАНИЕ ВОЗИОЖНО ТОЛЬКО, ЕСЛИ ДАТАСЕТ СОДЕРЖИТ ТОЛЬКО ОДИН ПРЕДИКТОР (ОН ЖЕ ПРОГНОЗИРУЕМАЯ ВЕЛИЧИНА),
# ИНАЧЕ 
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
    parser.add_argument('--json_file_path',type=str,default='E:\Anton\Desktop\MAIN\Optimization\Easy\Easy[0]\h.json')
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
except Exception as e:
    print(str(e))
    try:
        logErrorPath = "log_error.txt"
        logPath = "log.txt"
        sys.stderr = open(logErrorPath, 'w')
        logFile = open(logPath,"w")
        logFile.write(logPath)
        logFile.close()
    except Exception as e:
        print(str(e))
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
log('model loaded')
log("save_path: " + save_path)
log("window_size: " + (str)(window_size)) 

inputFile = open(h("input_file/value"))
#превращение входного файла в плоскую таблицу значений предикторов
lines = inputFile.readlines()
# predicted_column_index ВСЕГДА БУДЕТ РАВЕН 0 (см. заголовок скрипта)
predicted_column_index = (int)(h("predicted_column_index/value"))

if (h("NN_struct/layer1/value") == "LSTM") | (h("NN_struct/layer1/value") == "Conv1D"):
    X = numpy.zeros((1, window_size,1), dtype=numpy.float32)
else:
    X = numpy.zeros((1, window_size), dtype=numpy.float32)

log("X.shape: " + (str)(X.shape))

############
cycles = 50
############

predictionsFile = open(os.path.dirname(args.json_file_path) + "\\cyclic_prediction.txt", 'w')
head = lines[0].split(';')[predicted_column_index]
  
#log(h("predictions_file_path"))
# predicted_column_index ВСЕГДА БУДЕТ РАВЕН 0 (см. заголовок скрипта)
head = head = lines[0].split(';')[predicted_column_index].replace('\n','') + ';' + '(predicted -> )' + lines[0].split(';')[(int)(h("predicted_column_index/value"))].replace('\n','') + ';type'
# if head[:-1]==';': 6
#     head = head[0:-1]
predictionsFile.write(head + '\n')

start_point = (int)(len(lines) * (float)(h('split_point/value'))) + 10

# до введения переменной steps_forward размер массива data совпадал с
# window_size,
# теперь массив data длиннее, чем window_size на (steps_forward-1)
data = []
try:
    steps_forward = (int)(h("steps_forward/value"))
except:
    steps_forward = 1

for i in range(0,window_size + (steps_forward - 1)):
    # заполнение первого окна
    data.append(lines[start_point + i + 1])
    line = ''
    line = line + (str)(lines[start_point + i + 1]).replace('\n','') + ';' + lines[start_point + steps_forward + i + 1].split(';')[(int)(h("predicted_column_index/value"))].replace('\n','') + '; real (1st window)'
    predictionsFile.write(line + '\n')

for c in range(0,cycles):
    for i in range(0,window_size):
    
        if (h("NN_struct/layer1/value") == "LSTM") | (h("NN_struct/layer1/value") == "Conv1D"):
            for j in range(0,len(data[i].split(';'))):   
                featureStringValue = data[i].split(';')[j]
                if featureStringValue != '\n':     
                    X[0,i ,j] = (float)(data[i].split(';')[j])
        else:
            # predicted_column_index ВСЕГДА БУДЕТ РАВЕН 0 (см. заголовок скрипта) 
             X[0,i] = (float)(data[i].split(';')[predicted_column_index])


    Y = model.predict(X)

    data = numpy.delete(data, 0)
    data = numpy.append(data,(str)(Y[0,0]))

    line = ''
    line = line + (str)(lines[start_point + (window_size + (steps_forward - 1)) + 1 + c]).replace('\n','') + ';'
    line = line + (str)(Y[0,0]) + '; '
    line = line + (str)(c) + ' prediction'
    predictionsFile.write(line + '\n')



predictionsFile.close()
log("____END____")