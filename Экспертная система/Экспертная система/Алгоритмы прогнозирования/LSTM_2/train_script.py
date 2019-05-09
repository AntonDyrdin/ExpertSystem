prediction_algorithm_name = 'LSTM_2'
def log(s):
    print(s)
log("СКРИПТ ОБУЧЕНИЯ " + prediction_algorithm_name + " ЗАПУЩЕН...") 
import time
tempTime = time.time()
def getTime():
    global tempTime 
    offset = time.time() - tempTime
    tempTime = time.time()
    return str( offset )[0:5]+" сек."
    
from cntk.device import try_set_default_device, gpu
import cntk.device as C
print(C.all_devices())
print(C.try_set_default_device(C.gpu(0)))
print(C.use_default_device())        
import argparse
import numpy
import json
from keras.models import Sequential
from keras.layers import Dense
from keras.layers import LSTM
from keras.layers import Dropout

log("> время загрузки библиотек : "+ getTime())  


def createParser():
    parser = argparse.ArgumentParser()
    parser.add_argument('--json_file_path',type=str,default='D:\Anton\Desktop\MAIN\Экспертная система\Экспертная система\Алгоритмы прогнозирования\LSTM_1\json.txt')
    # parser.add_argument('--json_file_path',type=str,default='C:\Users\anton\Рабочий стол\MAIN\Экспертная система\Экспертная система\Алгоритмы прогнозирования\LSTM 1\json.txt')
    return parser
  
def h(nodeName):
    return  jsonObj[baseNodeName][nodeName]["value"]

def h2(nodeName1,nodeName2):
    return  jsonObj[baseNodeName][nodeName1][nodeName2]["value"]

def getAttr2(nodeName1,nodeName2,attrName):
    return  jsonObj[baseNodeName][nodeName1][nodeName2][attrName]

def getAttr2int(nodeName1,nodeName2,attrName):
    return  (int)(jsonObj[baseNodeName][nodeName1][nodeName2][attrName])
def h3(nodeName1,nodeName2,nodeName3):
    return  jsonObj[baseNodeName][nodeName1][nodeName2][nodeName3]["value"]
def h3INT(nodeName1,nodeName2,nodeName3):
    return  (int)(jsonObj[baseNodeName][nodeName1][nodeName2][nodeName3]["value"])
def h3FLOAT(nodeName1,nodeName2,nodeName3):
    return  (float)(jsonObj[baseNodeName][nodeName1][nodeName2][nodeName3]["value"])

parser = createParser()
args = parser.parse_args()
jsonFile = open(args.json_file_path, 'r')
jsontext = jsonFile.read()
jsonFile.close()
jsonObj = json.loads(jsontext)
#print(json.dumps(jsonObj,indent=12,ensure_ascii=False))  
#
baseNodeName=  next((v for i, v in enumerate(jsonObj.items()) if i == 0))[0]

inputFile = open(h("input_file"))
           
allLines = inputFile.readlines()
dataset = numpy.zeros((len(allLines) - 1, len(allLines[0].split(';'))),dtype=float)
window_size = (int)(h("window_size"))
for i in range(1,len(allLines)):
    for j in range(0,len(allLines[i].split(';'))):   
        featureStringValue = allLines[i].split(';')[j]
        if featureStringValue != '\n':     
            dataset[i - 1,j] = (float)(allLines[i].split(';')[j])

print(dataset.shape)
Dataset_X = numpy.zeros((dataset.shape[0] - window_size, window_size,dataset.shape[1]), dtype=float)
Dataset_Y = numpy.zeros(dataset.shape[0] - window_size, dtype=float)
predicted_column_index = (int)(h("predicted_column_index"))
for i in range(0,dataset.shape[0] - window_size):
    for j in range(0,window_size):
        for k in range(0,dataset.shape[1]):
            Dataset_X[i,j,k] = dataset[i + j][k]
    Dataset_Y[i] = dataset[i + window_size,predicted_column_index]
train_start_point = (int)((float)(h("start_point"))*Dataset_X.shape[0])
split_point = (float)(h("split_point"))
train_X = Dataset_X[train_start_point:round(Dataset_X.shape[0] * (split_point)), :,:]
test_X = Dataset_X[round(Dataset_X.shape[0] * (split_point)):, :,:]
train_y = Dataset_Y[train_start_point:round(Dataset_Y.shape[0] * (split_point)):]
test_y = Dataset_Y[round(Dataset_Y.shape[0] * (split_point)):]

print("> время чтения данных  : ", getTime())  

model = Sequential()         

model.add(LSTM(h3INT("NN_sctruct","layer1","neurons_count"),return_sequences=True, input_shape=(train_X.shape[1], train_X.shape[2])))
model.add(Dropout(h3FLOAT("NN_sctruct","layer2","dropout")))
model.add(LSTM(h3INT("NN_sctruct","layer3","neurons_count"),return_sequences=False,activation=h3("NN_sctruct","layer3","activation")))
model.add(Dropout(h3FLOAT("NN_sctruct","layer4","dropout")))
model.add(Dense(h3INT("NN_sctruct","layer5","neurons_count")))
model.add(Dropout(h3FLOAT("NN_sctruct","layer6","dropout")))
model.add(Dense(1,activation=h3("NN_sctruct","layer7","activation")))
                                                                  
log("компиляция НС...")
        
model.compile(loss=h("loss"), optimizer=h("optimizer"),metrics=['accuracy'])
log("> время компиляции НС  : "+ getTime())    


    
log("обучение НС...")
        
history = model.fit(train_X, train_y, epochs=(int)(h("number_of_epochs")), batch_size=(int)(h("batch_size")), validation_data=(test_X, test_y), shuffle=True) 
log("> время обучения  НС  : "+ getTime()) 
    
if h("save_folder") != "none":
    save_path = h("save_folder")+ 'weights.h5' 
    log("сохранение модели: " + h("save_folder")+ 'weights.h5')
    try:
        model.save(save_path)
    except:
        save_path=save_path.encode('ansi')
        model.save(save_path)
    log("> время сохранения НС  : "+ getTime()) 
    
sum = 0    
    
log("создание тестового прогноза")
predicted = model.predict(test_X)
log(predicted.shape)
for i in range(0,test_X.shape[0]):
    sum = sum + predicted[i,0]
avg = sum / predicted.shape[0]



for i in range(0,test_X.shape[0]):
    predicted[i,0] = predicted[i,0] - avg
    predicted[i,0] = predicted[i,0] * 1000
    predicted[i,0] = predicted[i,0] + 0.5
predictionsFile = open(h("predictions_file_path"), 'w')
head = ''
for i in range(0,len(allLines[0].split(';'))):
    head = head + allLines[0].split(';')[i] + ';'

head = head[0:-1]
head = head.replace('\n',';')
#log(h("predictions_file_path"))
head = head + '(predicted -> )' + allLines[0].split(';')[(int)(h("predicted_column_index"))] 
# if  head[:-1]==';':
#     head = head[0:-1]
predictionsFile.write(head +'\n')
print("test_X.shape",test_X.shape )
for i in range(0,test_X.shape[0]):
    line = ''
    for k in range(0,test_X.shape[2]): 
        line = line + (str)(test_X[i,window_size - 1,k]) + ';'
    line = line + (str)(predicted[i,0])
    predictionsFile.write(line + '\n')
predictionsFile.close()
log("> время создания и записи тестового прогноза  : "+ getTime()) 
log("__________________________________")    
log("______________END________________")     
RESPONSE="{RESPONSE:{"
RESPONSE=RESPONSE+ "AVG:{value:"+(str)(avg)
RESPONSE=RESPONSE+ "}}}"
print(RESPONSE)


if h("show_train_charts")=="True":
    import matplotlib.pyplot as pyplot
    pyplot.plot(history.history['loss'], label='train')
    pyplot.plot(history.history['val_loss'], label='test')
    pyplot.legend()
    pyplot.show()
  
    pyplot.plot(history.history['acc'], label='acc')
    pyplot.plot(history.history['val_acc'], label='val_acc')
    pyplot.legend()
    pyplot.show()