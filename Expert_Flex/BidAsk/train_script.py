﻿prediction_algorithm_name = 'BidAsk'

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
log("Все вычислительные устройства: " + str(C.all_devices()))
try:
    log("Попытка установить GPU как устройство по умолчанию: " + str(C.try_set_default_device(C.gpu(0))))
except Exception as e:
    log(str(e))   
#log(C.use_default_device())
###################################################

#  загрузка библиотек
import numpy
import json
import math
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

# если первый слой рекуррентный, то построть трёхмерный массив - иначе плоскую
# матрицу
inc001 = 0
inc010 = 0
inc100 = 0
incAll = 0

bid_column_index = (int)(h("bid_column_index/value"))
ask_column_index = (int)(h("ask_column_index/value"))
spread_column_index = dataset.shape[1] - 1

# подсчёт количества элементов классов для исключения парадокса точности
# обучения из-за неравномерного распределения выборки

# wait_for_rise - количество шагов в будущее, в пределах которых ожидается
# изменение
wait_for_rise = (int)(h("wait_for_rise/value"))
for i in range(0,dataset.shape[0] - window_size):
    spread = dataset[i + window_size - 1,spread_column_index]
    delta_bid = 0
    delta_ask = 0
    which_class = 0
    incAll = incAll + 1
    for k in range(0,wait_for_rise):
        if  (i + k + window_size) < (dataset.shape[0] - window_size):
            x = dataset[i + window_size + k,bid_column_index]
            delta_bid = delta_bid + math.tan((x - 0.5) * math.pi)

            x = dataset[i + window_size + k,ask_column_index]
            delta_ask = delta_ask + math.tan((x - 0.5) * math.pi)

            if  delta_bid > spread * 1.2:
                which_class = 2
                break

            if delta_ask < -spread:
                which_class = 1
                break
                    
    if which_class == 0:
        inc100 = inc100 + 1
    if which_class == 1:
        inc010 = inc010 + 1
    if which_class == 2:
        inc001 = inc001 + 1
log("[0;0;1] = " + (str)(inc001) + "    " + (str)(100 * (inc001 / incAll)) + " %")
log("[0;1;0] = " + (str)(inc010) + "    " + (str)(100 * (inc010 / incAll)) + " %")
log("[1;0;0] = " + (str)(inc100) + "    " + (str)(100 * (inc100 / incAll)) + " %")
log("Всего = " + (str)(incAll))

#количество повторений редких классов
K_resample = 0
#  количество зарезервированных мест в датасете под ресемплирование
#  (дублирование редких классов)
N_resample = (inc001 + inc010) * K_resample

log("под ресемплирование зарезервировано " + (str)(N_resample) + " мест")

spreads = numpy.zeros((dataset.shape[0] - window_size + N_resample), dtype=numpy.float32)

if (h("NN_struct/layer1/value") == "LSTM") | (h("NN_struct/layer1/value") == "Conv1D"):

    Dataset_X = numpy.zeros((dataset.shape[0] - window_size + N_resample, window_size,dataset.shape[1] - 1), dtype=numpy.float32)
    Dataset_Y = numpy.zeros((dataset.shape[0] - window_size + N_resample,3), dtype=numpy.float32)
    #чтобы образцы двух классов не скапливались в конце и не попадали в
    #тестировочную выборку, нужно их размещать в начале, а оригинальный порядок
    #следования образцов размещать со смещением в resamlpe_shift
    resamlpe_shift = K_resample * (inc001 + inc010)

    for i in range(0,dataset.shape[0] - window_size):
        
        for j in range(0,window_size):
            for k in range(0,dataset.shape[1] - 1):
                Dataset_X[resamlpe_shift + i,j,k] = dataset[i + j][k]

        spread = dataset[i + window_size - 1,spread_column_index]
        spreads[resamlpe_shift + i] = spread
        
        delta_bid = 0
        delta_ask = 0
        which_class = 0
        incAll = incAll + 1
        for k in range(0,wait_for_rise):
            if  (i + k + window_size) < (dataset.shape[0] - window_size):
                #обратная функция от сигмоиды - arctanh((x)-0.5)*2
                x = dataset[i + window_size + k,bid_column_index]
                delta_bid = delta_bid + math.tan((x - 0.5) * math.pi)
                #обратная функция от сигмоиды - arctanh((x)-0.5)*2
                x = dataset[i + window_size + k,ask_column_index]
                delta_ask = delta_ask + math.tan((x - 0.5) * math.pi)
                #   если на следующем шаге графика бид (цена, по которой можно
                #   будет
                #   продать) превысит аск на настоящем шаге(цена, по которой
                #   будем покупать
                #   сейчас), то Y=[0;0;1] - вход на рынок
                if  delta_bid > spread * 1.2:
                    which_class = 2
                    break
                #   если на следующем шаге графика аск (цена предложения)
                #   упадёт
                #   ниже,
                #   чем бид сейчас (цена спроса), то Y=[0;1;0] - выход с
                #   рынка
                if delta_ask < -spread:
                    which_class = 1
                    break
                    
        if which_class == 2:
            Dataset_Y[resamlpe_shift + i,0] = 0
            Dataset_Y[resamlpe_shift + i,1] = 0  
            Dataset_Y[resamlpe_shift + i,2] = 1

        if which_class == 1:
            Dataset_Y[resamlpe_shift + i,0] = 0
            Dataset_Y[resamlpe_shift + i,1] = 1  
            Dataset_Y[resamlpe_shift + i,2] = 0

        if which_class == 0:
            Dataset_Y[resamlpe_shift + i,0] = 1
            Dataset_Y[resamlpe_shift + i,1] = 0  
            Dataset_Y[resamlpe_shift + i,2] = 0

    ###########   ресемплирование ##########
    for i in range(0,K_resample):
        inc001_ind = 0
        inc010_ind = 0
        for j in range(resamlpe_shift,resamlpe_shift + dataset.shape[0] - window_size):

            ind = (i * (inc001 + inc010)) + (inc010_ind + inc001_ind)

            if Dataset_Y[j,1] == 1:
                #копирование элементов класса [0 1 0]

                Dataset_X[ind] = Dataset_X[j] 
                Dataset_Y[ind] = Dataset_Y[j]
                spreads[ind] = spreads[j]
                inc010_ind = inc010_ind + 1

            if Dataset_Y[j,2] == 1:
                #копирование элементов класса [0 0 1]
                Dataset_X[ind] = Dataset_X[j] 
                Dataset_Y[ind] = Dataset_Y[j]
                spreads[ind] = spreads[j]
                inc001_ind = inc001_ind + 1
                
    ######################################################################

    #разбиение на обучающую и тестовую выборки
    train_X = Dataset_X[:round(Dataset_X.shape[0] * (split_point)), :,:]
    test_X = Dataset_X[round(Dataset_X.shape[0] * (split_point)):, :,:]
    train_y = Dataset_Y[:round(Dataset_Y.shape[0] * (split_point)):]
    test_y = Dataset_Y[round(Dataset_Y.shape[0] * (split_point)):]
    test_spreads = spreads[round(spreads.shape[0] * (split_point)):]
else:
    Dataset_X = numpy.zeros((dataset.shape[0] ,dataset.shape[1]), dtype=numpy.float32)
    Dataset_Y = numpy.zeros((dataset.shape[0] ,3), dtype=numpy.float32)
    bid_column_index = (int)(h("bid_column_index/value"))
    ask_column_index = (int)(h("ask_column_index/value"))
    for i in range(0,dataset.shape[0] - window_size):
        for k in range(0,dataset.shape[1]):
            Dataset_X[i,j] = dataset[i + 1][k] 

        if(dataset[i + 1,bid_column_index]) > (dataset[i ,ask_column_index]):
            Dataset_Y[i,0] = 0
            Dataset_Y[i,1] = 0  
            Dataset_Y[i,2] = 1
        else:
            if(dataset[i + 1,ask_column_index]) < (dataset[i ,bid_column_index]):
                Dataset_Y[i,0] = 0
                Dataset_Y[i,1] = 1  
                Dataset_Y[i,2] = 0
            else:
                Dataset_Y[i,0] = 1
                Dataset_Y[i,1] = 0  
                Dataset_Y[i,2] = 0

    train_X = Dataset_X[:round(Dataset_X.shape[0] * (split_point)), :]
    test_X = Dataset_X[round(Dataset_X.shape[0] * (split_point)):, :]
    train_y = Dataset_Y[:round(Dataset_Y.shape[0] * (split_point)):,:]
    test_y = Dataset_Y[round(Dataset_Y.shape[0] * (split_point)):,:]
####################################################################################
#print("train_X ",train_X)
#print("test_X ",test_X)
#print("train_y ",train_y)
#print("test_y ",test_y)
log("> время чтения данных  : " + (str)(getTime()))  
log("train_X.shape" + (str)(train_X.shape))
#if (h("NN_struct/layer1/value") == "LSTM") | (h("NN_struct/layer1/value") ==
#"Conv1D"):
#    log((str)(train_X[0,:,:]))
#else:
#    log((str)(train_X[0,:]))
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
            log("add Conv1D layer " + h("NN_struct/" + LAYERS[i] + "/neurons_count/value") + " neurons, input_shape=( " + (str)(train_X.shape[1]) + ", " + (str)(train_X.shape[2]) + "), kernel_size=" + h("NN_struct/" + LAYERS[i] + "/kernel_size/value") + ", activation - relu")
            model.add(Conv1D((int)(h("NN_struct/" + LAYERS[i] + "/neurons_count/value")),(int)(h("NN_struct/" + LAYERS[i] + "/kernel_size/value")),activation='relu',input_shape=(train_X.shape[1],train_X.shape[2])))

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


head = "<bid_top>;<ask_top>;<spread>;<prediction>"

predictionsFile.write(head + '\n')


if (h("NN_struct/layer1/value") == "LSTM") | (h("NN_struct/layer1/value") == "Conv1D"):
    for i in range(0,test_X.shape[0]):
        line = ''
        line = line + (str)(math.tan((test_X[i,window_size - 1,bid_column_index] - 0.5) * math.pi)) + ';'
        line = line + (str)(math.tan((test_X[i,window_size - 1,ask_column_index] - 0.5) * math.pi)) + ';'
        line = line + (str)(test_spreads[i]) + ';'
        if (predicted[i,2] > predicted[i,1]) & (predicted[i,2] > predicted[i,0]):
            line = line + "1"
        if (predicted[i,1] > predicted[i,2]) & (predicted[i,1] > predicted[i,0]):
            line = line + "0"
        if (predicted[i,0] > predicted[i,1]) & (predicted[i,0] > predicted[i,2]):
            line = line + "0.5"
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
  
    pyplot.plot(history.history['acc'], label='accuracy')
    pyplot.plot(history.history['val_acc'], label='val_accuracy')
    pyplot.legend()
    pyplot.show()

RESPONSE = "{RESPONSE:{"
RESPONSE = RESPONSE + "response:{value:скрипт " + prediction_algorithm_name + " успешно завершён"
RESPONSE = RESPONSE + "}}}"
log(RESPONSE)
sys.stderr.close()