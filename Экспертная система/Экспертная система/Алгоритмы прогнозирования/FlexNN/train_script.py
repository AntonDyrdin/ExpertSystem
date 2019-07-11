prediction_algorithm_name = 'FlexNN'

# лог
def log(s):
    print(s)
############
log("СКРИПТ ОБУЧЕНИЯ " + prediction_algorithm_name + " ЗАПУЩЕН...") 

# секундомер
import time
tempTime = time.time()
def getTime():
    global tempTime 
    offset = time.time() - tempTime
    tempTime = time.time()
    return str(offset)[0:5] + " сек."
#####################################

# попытка задать GPU, как устройство для вычислений
from cntk.device import try_set_default_device, gpu
import cntk.device as C
print(C.all_devices())
print(C.try_set_default_device(C.gpu(0)))
print(C.use_default_device())  
###################################################

#  загрузка библиотек
import argparse
import numpy
import json
from keras.models import Sequential
from keras.layers import Dense
from keras.layers import LSTM
from keras.layers import Dropout
from keras.layers import Embedding
from keras.layers import Conv1D, GlobalAveragePooling1D, MaxPooling1D
#####################################################################
log("> время загрузки библиотек : " + getTime())  

# чтение параметров командной строки
def createParser():
    parser = argparse.ArgumentParser()
    parser.add_argument('--json_file_path',type=str,default='E:\Anton\Desktop\MAIN\Экспертная система\Экспертная система\Алгоритмы прогнозирования\LSTM_2\json.txt')
    return parser
#####################################################################
 

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

parser = createParser()
args = parser.parse_args()
jsonFile = open(args.json_file_path, 'r')
jsontext = jsonFile.read()
jsonFile.close()
jsonObj = json.loads(jsontext)
#print(json.dumps(jsonObj,indent=12,ensure_ascii=False))
#
baseNodeName = next((v for i, v in enumerate(jsonObj.items()) if i == 0))[0]

inputFile = open(h("input_file/value"))
#превращение входного файла в плоскую таблицу значений предикторов           
allLines = inputFile.readlines()
dataset = numpy.zeros((len(allLines) - 1, len(allLines[0].split(';'))),dtype=numpy.float32)
window_size = (int)(h("window_size/value"))
for i in range(1,len(allLines)):
    for j in range(0,len(allLines[i].split(';'))):   
        featureStringValue = allLines[i].split(';')[j]
        if featureStringValue != '\n':     
            dataset[i - 1,j] = (float)(allLines[i].split(';')[j])

# train_start_point определяет процент данных, которые будут отброшены
train_start_point = (int)((float)(h("start_point/value")) * dataset.shape[0])
dataset=dataset[train_start_point:,:]

print("dataset.shape: ",dataset.shape)   



Dataset_X = numpy.zeros((dataset.shape[0] - window_size, window_size,dataset.shape[1]), dtype=numpy.float32)
# выходной вектор состоит из двух значений
#принцип классификации: выходной слой - 2 нейрона, если значение второго больше первого - прогноз роста, иначе - прогноз снижения.

Dataset_Y = numpy.zeros(dataset.shape[0] - window_size,2, dtype=numpy.float32)
predicted_column_index = (int)(h("predicted_column_index/value"))
for i in range(0,dataset.shape[0] - window_size):
    for j in range(0,window_size):
        for k in range(0,dataset.shape[1]):
            Dataset_X[i,j,k] = dataset[i + j][k]
#   при убывающем тренде вектор Y=[1,0],
    if(dataset[i + window_size,predicted_column_index])>0:
        Dataset_Y[i,0] = 0
        Dataset_Y[i,1] = 1    
#   при растущем тренде Y=[0,1].
    if(dataset[i + window_size,predicted_column_index])<=0:
        Dataset_Y[i,0] = 1
        Dataset_Y[i,1] = 0 


#разбиение на обучающую и тестовую выборки 
split_point = (float)(h("split_point/value"))
train_X = Dataset_X[train_start_point:round(Dataset_X.shape[0] * (split_point)), :,:]
test_X = Dataset_X[round(Dataset_X.shape[0] * (split_point)):, :,:]
train_y = Dataset_Y[train_start_point:round(Dataset_Y.shape[0] * (split_point)):]
test_y = Dataset_Y[round(Dataset_Y.shape[0] * (split_point)):]
####################################################################################
print("> время чтения данных  : ", getTime())  

model = Sequential()         

LAYERS=h("NN_struct")

for layerNODE in LAYERS:
    if(h("NN_struct/"+layerNODE+"/value")=="Dense"):
        model.add(Dense(h("NN_struct/"+layerNODE+"/neurons_count/value")))

    if(h("NN_struct/"+layerNODE+"/value")=="LSTM"):
        model.add(LSTM(h("NN_struct/"+layerNODE+"/neurons_count/value")))

    if(h("NN_struct/"+layerNODE+"/value")=="Conv1D"):
        model.add(Conv1D(h("NN_struct/"+layerNODE+"/neurons_count/value")))

    if(h("NN_struct/"+layerNODE+"/value")=="MaxPooling1D"):
        model.add(MaxPooling1D(h("NN_struct/"+layerNODE+"/neurons_count/value")))

    if(h("NN_struct/"+layerNODE+"/value")=="GlobalAveragePooling1D"):
        model.add(GlobalAveragePooling1D(h("NN_struct/"+layerNODE+"/neurons_count/value")))

    if(h("NN_struct/"+layerNODE+"/value")=="Dropout"):
        model.add(Dropout(h("NN_struct/"+layerNODE+"/neurons_count/value")))

                                                                  
log("компиляция НС...")
        
model.compile(loss=h("loss/value"), optimizer=h("optimizer/value"),metrics=['accuracy'])
log("> время компиляции НС  : " + getTime())    


    
log("обучение НС...")
        
history = model.fit(train_X, train_y, epochs=(int)(h("number_of_epochs/value")), batch_size=(int)(h("batch_size/value")), validation_data=(test_X, test_y), shuffle=True) 
log("> время обучения  НС  : " + getTime()) 
    
if h("save_folder/value") != "none":
    save_path = h("save_folder/value") + 'weights.h5' 
    log("сохранение модели: " + h("save_folder/value") + 'weights.h5')
    try:
        model.save(save_path)
    except:
        save_path = save_path.encode('ansi')
        model.save(save_path)
    log("> время сохранения НС  : " + getTime()) 
    
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
predictionsFile = open(h("predictions_file_path/value"), 'w')
head = ''
for i in range(0,len(allLines[0].split(';'))):
    head = head + allLines[0].split(';')[i] + ';'

head = head[0:-1]
head = head.replace('\n',';')
#log(h("predictions_file_path"))
head = head + '(predicted -> )' + allLines[0].split(';')[(int)(h("predicted_column_index"))] 
# if head[:-1]==';':
#     head = head[0:-1]
predictionsFile.write(head + '\n')
print("test_X.shape",test_X.shape)
for i in range(0,test_X.shape[0]):
    line = ''
    for k in range(0,test_X.shape[2]): 
        line = line + (str)(test_X[i,window_size - 1,k]) + ';'
    line = line + (str)(predicted[i,0])
    predictionsFile.write(line + '\n')
predictionsFile.close()
log("> время создания и записи тестового прогноза  : " + getTime()) 
log("__________________________________")    
log("______________END________________")     
RESPONSE = "{RESPONSE:{"
RESPONSE = RESPONSE + "AVG:{value:" + (str)(avg)
RESPONSE = RESPONSE + "}}}"
print(RESPONSE)


if h("show_train_charts/value") == "True":
    import matplotlib.pyplot as pyplot
    pyplot.plot(history.history['loss'], label='train')
    pyplot.plot(history.history['val_loss'], label='test')
    pyplot.legend()
    pyplot.show()
  
    pyplot.plot(history.history['acc'], label='acc')
    pyplot.plot(history.history['val_acc'], label='val_acc')
    pyplot.legend()
    pyplot.show()