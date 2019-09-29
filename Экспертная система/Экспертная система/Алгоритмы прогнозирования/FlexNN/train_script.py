prediction_algorithm_name = 'FlexNN'

# чтение параметров командной строки
import argparse
def createParser():
    parser = argparse.ArgumentParser()
    parser.add_argument('--json_file_path',type=str,default='h.json')
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

# попытка задать GPU, как устройство для ускорения вычислений
from cntk.device import try_set_default_device, gpu
import cntk.device as C
log("Все вычислительные устройства: "+str(C.all_devices()))
try:
    log("Попытка установить GPU как устройство по умолчанию: "+str(C.try_set_default_device(C.gpu(0))))
except Exception as e:
    log(str(e))   
#log(C.use_default_device())  
###################################################

#  загрузка библиотек
import numpy
import json
from keras.models import Sequential
from keras.layers import Dense, LSTM, Dropout, Conv1D, GlobalAveragePooling1D, MaxPooling1D, Flatten
from keras import optimizers
#####################################################################
log("> время загрузки библиотек : " + getTime())  


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


jsonFile = open(args.json_file_path, 'r')
jsontext = jsonFile.read()
jsonFile.close()
jsonObj = json.loads(jsontext)
#log(json.dumps(jsonObj,indent=12,ensure_ascii=False))
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
dataset = dataset[train_start_point:,:]

log("dataset.shape: " + (str)(dataset.shape))   

split_point = (float)(h("split_point/value"))

# если первый слой рекуррентный, то построть трёхмерный массив - иначе плоскую матрицу
if (h("NN_struct/layer1/value") == "LSTM") | (h("NN_struct/layer1/value") == "Conv1D"):

    Dataset_X = numpy.zeros((dataset.shape[0] - window_size, window_size,dataset.shape[1]), dtype=numpy.float32)
    # выходной вектор состоит из двух значений
    #принцип классификации: выходной слой - 2 нейрона, если значение второго
    #больше первого - прогноз роста, иначе - прогноз снижения.

    Dataset_Y = numpy.zeros((dataset.shape[0] - window_size,2), dtype=numpy.float32)
    predicted_column_index = (int)(h("predicted_column_index/value"))
    for i in range(0,dataset.shape[0] - window_size):
        for j in range(0,window_size):
            for k in range(0,dataset.shape[1]):
                Dataset_X[i,j,k] = dataset[i + j][k]
    #   при растущем тренде Y=[0,1].
        if(dataset[i + window_size,predicted_column_index]) > 0.5:
            Dataset_Y[i,0] = 0
            Dataset_Y[i,1] = 1    
#   при убывающем тренде вектор Y=[1,0],
        if(dataset[i + window_size,predicted_column_index]) < 0.5:
            Dataset_Y[i,0] = 1
            Dataset_Y[i,1] = 0 

    #разбиение на обучающую и тестовую выборки

    train_X = Dataset_X[:round(Dataset_X.shape[0] * (split_point)), :,:]
    test_X = Dataset_X[round(Dataset_X.shape[0] * (split_point)):, :,:]
    train_y = Dataset_Y[:round(Dataset_Y.shape[0] * (split_point)):]
    test_y = Dataset_Y[round(Dataset_Y.shape[0] * (split_point)):]
    
else:
    Dataset_X = numpy.zeros((dataset.shape[0] - window_size, window_size), dtype=numpy.float32)
    Dataset_Y = numpy.zeros((dataset.shape[0] - window_size,2), dtype=numpy.float32)
    predicted_column_index = (int)(h("predicted_column_index/value"))
    for i in range(0,dataset.shape[0] - window_size):
        for j in range(0,window_size):
                Dataset_X[i,j] = dataset[i + j][predicted_column_index] 
    #   при убывающем тренде вектор Y=[1,0],
        if(dataset[i + window_size,predicted_column_index]) > 0.5:
            Dataset_Y[i,0] = 0
            Dataset_Y[i,1] = 1    
    #   при растущем тренде Y=[0,1].
        if(dataset[i + window_size,predicted_column_index]) < 0.5:
            Dataset_Y[i,0] = 1
            Dataset_Y[i,1] = 0 

    train_X = Dataset_X[:round(Dataset_X.shape[0] * (split_point)), :]
    test_X = Dataset_X[round(Dataset_X.shape[0] * (split_point)):, :]
    train_y = Dataset_Y[:round(Dataset_Y.shape[0] * (split_point)):,:]
    test_y = Dataset_Y[round(Dataset_Y.shape[0] * (split_point)):,:]
####################################################################################
log("> время чтения данных  : " + (str)(getTime()))  
log("train_X.shape" + (str)(train_X.shape))
if (h("NN_struct/layer1/value") == "LSTM") | (h("NN_struct/layer1/value") == "Conv1D"):
    log((str)(train_X[0,:,:]))
else:
    log((str)(train_X[0,:]))
log("train_y.shape" + (str)(train_y.shape))
if (h("NN_struct/layer1/value") == "LSTM") | (h("NN_struct/layer1/value") == "Conv1D"):
    log((str)(train_y[0]))
else:
    log((str)(train_y[0,:]))
model = Sequential()         

LAYERS = list(h("NN_struct").keys())

isFirst = True


for i in range(0,len(LAYERS)):
    if isFirst:
        if(h("NN_struct/" + LAYERS[i] + "/value") == "Dense"):
            log("add Dense layer " + h("NN_struct/" + LAYERS[i] + "/neurons_count/value") + " neurons, input_dim=" + (str)(window_size) + ", activation: " + h("NN_struct/" + LAYERS[i] + "/activation/value"))
            model.add(Dense((int)(h("NN_struct/" + LAYERS[i] + "/neurons_count/value")),input_dim=window_size,activation=h("NN_struct/" + LAYERS[i] + "/activation/value")))

        if  h("NN_struct/" + LAYERS[i] + "/value") == "LSTM":
               if(i < len(LAYERS) - 1):
                    # если следующий слой тоже рекуррентный - return_sequens =
                    # True
                    if(h("NN_struct/layer" + (str)(i + 2) + "/value") == "LSTM"):
                        log("add LSTM layer " + h("NN_struct/" + LAYERS[i] + "/neurons_count/value") + " neurons, input_shape=( " + (str)(train_X.shape[1]) + ", " + (str)(train_X.shape[2]) + "), return_sequences=True")
                        model.add(LSTM((int)(h("NN_struct/" + LAYERS[i] + "/neurons_count/value")),return_sequences=True, input_shape=(train_X.shape[1], train_X.shape[2])))
                    else:
                            if(LAYERS.index(LAYERS[i]) < len(LAYERS) - 2):
                                if(h("NN_struct/layer" + (str)(i + 3) + "/value") == "LSTM"):
                                    log("add LSTM layer " + h("NN_struct/" + LAYERS[i] + "/neurons_count/value") + " neurons, input_shape=( " + (str)(train_X.shape[1]) + ", " + (str)(train_X.shape[2]) + "), return_sequences=True")
                                    model.add(LSTM((int)(h("NN_struct/" + LAYERS[i] + "/neurons_count/value")),return_sequences=True, input_shape=(train_X.shape[1], train_X.shape[2])))
                                else:
                                    log("add LSTM layer " + h("NN_struct/" + LAYERS[i] + "/neurons_count/value") + " neurons, input_shape=( " + (str)(train_X.shape[1]) + ", " + (str)(train_X.shape[2]) + "), return_sequences=False")
                                    model.add(LSTM((int)(h("NN_struct/" + LAYERS[i] + "/neurons_count/value")),return_sequences=False, input_shape=(train_X.shape[1], train_X.shape[2])))
                            else:
                                log("add LSTM layer " + h("NN_struct/" + LAYERS[i] + "/neurons_count/value") + " neurons, input_shape=( " + (str)(train_X.shape[1]) + ", " + (str)(train_X.shape[2]) + "), return_sequences=False")
                                model.add(LSTM((int)(h("NN_struct/" + LAYERS[i] + "/neurons_count/value")),return_sequences=False, input_shape=(train_X.shape[1], train_X.shape[2])))
               else:
                    log("add LSTM layer " + h("NN_struct/" + LAYERS[i] + "/neurons_count/value") + " neurons, input_shape=( " + (str)(train_X.shape[1]) + ", " + (str)(train_X.shape[2]) + "), return_sequences=False")
                    model.add(LSTM((int)(h("NN_struct/" + LAYERS[i] + "/neurons_count/value")),return_sequences=False, input_shape=(train_X.shape[1], train_X.shape[2])))
        if(h("NN_struct/" + LAYERS[i] + "/value") == "Conv1D"):
            log("add Conv1D layer " + h("NN_struct/" + LAYERS[i] + "/neurons_count/value") + " neurons, input_shape=( " + (str)(window_size) + ", " + (str)(train_X.shape[1]) + "), kernel_size=" + h("NN_struct/" + LAYERS[i] + "/kernel_size/value") + ", activation - relu")
            model.add(Conv1D((int)(h("NN_struct/" + LAYERS[i] + "/neurons_count/value")),(int)(h("NN_struct/" + LAYERS[i] + "/kernel_size/value")),activation='relu',input_shape=(window_size,dataset.shape[1])))

    else:
        if(h("NN_struct/" + LAYERS[i] + "/value") == "Dense"):
            log("add Dense layer " + h("NN_struct/" + LAYERS[i] + "/neurons_count/value") + " neurons, activation: " + h("NN_struct/" + LAYERS[i] + "/activation/value"))
            model.add(Dense((int)(h("NN_struct/" + LAYERS[i] + "/neurons_count/value")),activation=h("NN_struct/" + LAYERS[i] + "/activation/value")))
                                                                  
        if  h("NN_struct/" + LAYERS[i] + "/value") == "LSTM":
            if(i < len(LAYERS) - 1):
                # если следующий слой тоже рекуррентный - return_sequens = True
                if(h("NN_struct/layer" + (str)(i + 2) + "/value") == "LSTM"):
                    log("add LSTM layer " + h("NN_struct/" + LAYERS[i] + "/neurons_count/value") + " neurons, return_sequences=True")
                    model.add(LSTM((int)(h("NN_struct/" + LAYERS[i] + "/neurons_count/value")),return_sequences=True))
                else:
                        if(LAYERS.index(LAYERS[i]) < len(LAYERS) - 2):
                            if(h("NN_struct/layer" + (str)(i + 3) + "/value") == "LSTM"):
                                log("add LSTM layer " + h("NN_struct/" + LAYERS[i] + "/neurons_count/value") + " neurons, return_sequences=True")
                                model.add(LSTM((int)(h("NN_struct/" + LAYERS[i] + "/neurons_count/value")),return_sequences=True))
                            else:
                                log("add LSTM layer " + h("NN_struct/" + LAYERS[i] + "/neurons_count/value") + " neurons, return_sequences=False")
                                model.add(LSTM((int)(h("NN_struct/" + LAYERS[i] + "/neurons_count/value")),return_sequences=False))
                        else:
                            log("add LSTM layer " + h("NN_struct/" + LAYERS[i] + "/neurons_count/value") + " neurons, return_sequences=False")
                            model.add(LSTM((int)(h("NN_struct/" + LAYERS[i] + "/neurons_count/value")),return_sequences=False))
            else:
                log("add LSTM layer " + h("NN_struct/" + LAYERS[i] + "/neurons_count/value") + " neurons, return_sequences=False")
                model.add(LSTM((int)(h("NN_struct/" + LAYERS[i] + "/neurons_count/value")),return_sequences=False))

        if(h("NN_struct/" + LAYERS[i] + "/value") == "Conv1D"):
            log("add Conv1D layer " + h("NN_struct/" + LAYERS[i] + "/neurons_count/value") + " neurons, kernel_size=" + h("NN_struct/" + LAYERS[i] + "/kernel_size/value") + ", activation - relu")
            model.add(Conv1D((int)(h("NN_struct/" + LAYERS[i] + "/neurons_count/value")),(int)(h("NN_struct/" + LAYERS[i] + "/kernel_size/value")),activation='relu'))

        if(h("NN_struct/" + LAYERS[i] + "/value") == "MaxPooling1D"):
            log("add MaxPooling1D layer, pool_size=" + h("NN_struct/" + LAYERS[i] + "/pool_size/value"))
            model.add(MaxPooling1D(pool_size=(int)(h("NN_struct/" + LAYERS[i] + "/pool_size/value"))))

        if(h("NN_struct/" + LAYERS[i] + "/value") == "GlobalAveragePooling1D"):
            log("add GlobalAveragePooling1D layer")
            model.add(GlobalAveragePooling1D())

        if(h("NN_struct/" + LAYERS[i] + "/value") == "Dropout"):
            log("add Dropout layer, dropout= " + h("NN_struct/" + LAYERS[i] + "/dropout/value"))
            model.add(Dropout((float)(h("NN_struct/" + LAYERS[i] + "/dropout/value"))))

        if(h("NN_struct/" + LAYERS[i] + "/value") == "Flatten"):
            log("add Flatten layer")
            model.add(Flatten())


    isFirst = False
                                                                  
log("компиляция НС...")
        
#optimizer=h("optimizer/value")
optimizer = optimizers.Adam(lr=(float)(h("learning_rate/value")), beta_1=0.9, beta_2=0.999, epsilon=None, decay=0.0, amsgrad=False)

model.compile(loss=h("loss/value"),optimizer=optimizer ,metrics=['accuracy'])
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
    
log("создание тестового прогноза")
predicted = model.predict(test_X)
log(predicted.shape)

predictionsFile = open(h("predictions_file_path/value"), 'w')
head = ''
for i in range(0,len(allLines[0].split(';'))):
    head = head + allLines[0].split(';')[i] + ';'

head = head[0:-1]
head = head.replace('\n',';')
#log(h("predictions_file_path"))
head = head + '(predicted -> )' + allLines[0].split(';')[(int)(h("predicted_column_index/value"))] 
# if head[:-1]==';':
#     head = head[0:-1]
predictionsFile.write(head + '\n')


if (h("NN_struct/layer1/value") == "LSTM")|(h("NN_struct/layer1/value") == "Conv1D"):
    for i in range(0,test_X.shape[0]):
        line = ''
        for k in range(0,test_X.shape[2]): 
            line = line + (str)(test_X[i,window_size - 1,k]) + ';'
        line = line + (str)(predicted[i,1] - predicted[i,0] + 0.5)
        predictionsFile.write(line + '\n')
else:
    for i in range(0,test_X.shape[0]):
        line = ''
        for k in range(0,dataset.shape[1]): 
            line = line + (str)(dataset[i,k]) + ';'
        line = line + (str)(predicted[i,1] - predicted[i,0] + 0.5)
        predictionsFile.write(line + '\n')

predictionsFile.close()
log("> время создания и записи тестового прогноза  : " + getTime()) 
log("__________________________________")    
log("______________END________________")     


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

RESPONSE = "{RESPONSE:{"
RESPONSE = RESPONSE + "response:{value:скрипт " + prediction_algorithm_name + " успешно завершён"
RESPONSE = RESPONSE + "}}}"
log(RESPONSE)
sys.stderr.close()