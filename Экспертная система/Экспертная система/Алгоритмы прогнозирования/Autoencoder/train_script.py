prediction_algorithm_name = 'Autoencoder'

# чтение параметров командной строки
import argparse
def createParser():
    parser = argparse.ArgumentParser()
    parser.add_argument('--json_file_path',type=str,default='E:\Anton\Desktop\MAIN\Экспертная система\Экспертная система\Алгоритмы прогнозирования\Autoencoder\h.json')
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
from keras.models import Sequential
from keras.layers import Dense, LSTM, Dropout, Conv1D, GlobalAveragePooling1D, MaxPooling1D, Flatten
from keras import optimizers
from keras.models import Model
#####################################################################
log("> время загрузки библиотек : " + getTime())  


jsonFile = open(args.json_file_path, 'r')
jsontext = jsonFile.read()
jsonFile.close()
jsonObj = json.loads(jsontext)
#log(json.dumps(jsonObj,indent=12,ensure_ascii=False))
#
baseNodeName = next((v for i, v in enumerate(jsonObj.items()) if i == 0))[0]

inputFile = open(h("input_file/value"))
try:
    log("CODE: " + h("code/value"))
except:
    log("CODE: null")
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
log("start point " + (str)(train_start_point))
dataset = dataset[train_start_point:,:]

log("dataset.shape: " + (str)(dataset.shape))   

split_point = (float)(h("split_point/value"))

batch_size = (int)(h("batch_size/value"))
predicted_column_index = (int)(h("predicted_column_index/value"))
# если первый слой рекуррентный, то построть трёхмерный массив - иначе плоскую
# матрицу
if (h("NN_struct/layer1/value") == "LSTM") | (h("NN_struct/layer1/value") == "Conv1D"):

    Dataset_X = numpy.zeros((dataset.shape[0] - window_size, window_size,dataset.shape[1]), dtype=numpy.float32)

    for i in range(0,dataset.shape[0] - window_size):
        for j in range(0,window_size):
            for k in range(0,dataset.shape[1]):
                Dataset_X[i,j,k] = dataset[i + j][k]

    #разбиение на обучающую и тестовую выборки
    train_X = Dataset_X[:round(Dataset_X.shape[0] * (split_point)), :,:]
    test_X = Dataset_X[round(Dataset_X.shape[0] * (split_point)):, :,:]

    # обрезание выборок, чтобы количество примеров в них было кратно batch_size
    #train_X_round_batch = (train_X.shape[0] // batch_size) * batch_size
    #test_X_round_batch = (test_X.shape[0] // batch_size) * batch_size
    #train_X = train_X[:train_X_round_batch, :,:]
    #test_X = test_X[:test_X_round_batch, :,:]
else:
    if(window_size == 1):
        Dataset_X = numpy.zeros((dataset.shape[0], dataset.shape[1]), dtype=numpy.float32)

        for i in range(0,dataset.shape[0]):
            for j in range(0,dataset.shape[1]):
                    Dataset_X[i,j] = dataset[i][j] 

            Dataset_Y[i] = dataset[i + 1,predicted_column_index]

        train_X = Dataset_X[:round(Dataset_X.shape[0] * (split_point)), :]
        test_X = Dataset_X[round(Dataset_X.shape[0] * (split_point)):, :]

    else:
        Dataset_X = numpy.zeros((dataset.shape[0] - window_size , window_size), dtype=numpy.float32)

        for i in range(0,dataset.shape[0] - window_size):
            for j in range(0,window_size):
                    Dataset_X[i,j] = dataset[i + j][predicted_column_index] 

        train_X = Dataset_X[:round(Dataset_X.shape[0] * (split_point)), :]
        test_X = Dataset_X[round(Dataset_X.shape[0] * (split_point)):, :]
####################################################################################
#print("train_X ",train_X)
#print("test_X ",test_X)

log("> время чтения данных  : " + (str)(getTime()))  
log("train_X.shape" + (str)(train_X.shape))
if (h("NN_struct/layer1/value") == "LSTM") | (h("NN_struct/layer1/value") == "Conv1D"):
    log((str)(train_X[0,:,:]))
else:
    log((str)(train_X[0,:]))

log("компиляция НС...")
     

LAYERS = list(h("NN_struct").keys())

isFirst = True
encoder = Sequential(name = "encoder")    
encoder.add(LSTM((int)(100),return_sequences=True,batch_input_shape=(batch_size, train_X.shape[1], train_X.shape[2]),activation="sigmoid"))
encoder.add(LSTM((int)(50),return_sequences=True,batch_input_shape=(batch_size, train_X.shape[1], train_X.shape[2]),activation="sigmoid"))
encoder.add(Dropout(0.1))
encoder.add(LSTM((int)(25),return_sequences=True,batch_input_shape=(batch_size, train_X.shape[1], train_X.shape[2]),activation="sigmoid"))
encoder.add(LSTM((int)(3),return_sequences=True,activation="sigmoid"))

decoder = Sequential(name = "decoder")
decoder.add(LSTM((int)(3),return_sequences=True,activation="sigmoid"))
decoder.add(LSTM((int)(25),return_sequences=True,batch_input_shape=(batch_size, train_X.shape[1], train_X.shape[2]),activation="sigmoid"))
decoder.add(Dropout(0.1))
decoder.add(LSTM((int)(50),return_sequences=True,batch_input_shape=(batch_size, train_X.shape[1], train_X.shape[2]),activation="sigmoid"))
decoder.add(LSTM((int)(6),return_sequences=True,activation="sigmoid"))
  
autoencoder = Sequential()
autoencoder.add(encoder)  
autoencoder.add(decoder)    
log("autoencoder.summary")
log(autoencoder.summary())
#optimizer=h("optimizer/value")
optimizer = optimizers.Adam(lr=(float)(h("learning_rate/value")), beta_1=0.9, beta_2=0.999, epsilon=None, decay=0.0, amsgrad=False)

autoencoder.compile(loss=h("loss/value"),optimizer=optimizer ,metrics=['accuracy'])
log("> время компиляции НС  : " + getTime())    

############# сохранение визуализации модели ################################
log("Cохранение визуализации модели")
os.environ["PATH"] += os.pathsep + 'C:/Program Files (x86)/Graphviz2.38/bin/'
from keras.utils import plot_model
plot_model(autoencoder, to_file=h("save_folder/value") + 'autoencoder.png', show_shapes='true')
plot_model(encoder, to_file=h("save_folder/value") + 'encoder.png', show_shapes='true')
#############################################################################
log("обучение НС...")
        
history = autoencoder.fit(train_X, train_X, epochs = (int)(h("number_of_epochs/value")), batch_size =batch_size, validation_data = (test_X, test_X), shuffle = True) 
log("> время обучения  НС  : " + getTime()) 
    
if h("save_folder/value") != "none":
    save_path = h("save_folder/value") + 'weights.h5' 
    log("сохранение модели: " + h("save_folder/value") + 'weights.h5')
    try:
        encoder.save(save_path)
    except:
        save_path = save_path.encode('ansi')
        encoder.save(save_path)
    log("> время сохранения НС  : " + getTime()) 

if h("show_train_charts/value") == "True":
    import matplotlib.pyplot as pyplot
    pyplot.plot(history.history['loss'], label='train')
    pyplot.plot(history.history['val_loss'], label='test')
    pyplot.legend()
    pyplot.show()
    
log("создание тестового прогноза")
predicted = encoder.predict(test_X, batch_size=batch_size)
log(predicted.shape)

predictionsFile = open(h("predictions_file_path/value"), 'w')
head = allLines[0].split(';')[predicted_column_index]

#log(h("predictions_file_path"))
head = head = allLines[0].replace('\n','') + ";<c>;<c1>;<c2>;<c3>;" 
# if head[:-1]==';':
#     head = head[0:-1]
predictionsFile.write(head + '\n')

log("predicted.shape")
log(predicted.shape)
log("predicted[0]")
log(predicted[0])

if (h("NN_struct/layer1/value") == "LSTM") | (h("NN_struct/layer1/value") == "Conv1D"):
    for i in range(0,test_X.shape[0]):
        line = ''
        for k in range(0,test_X.shape[2]): 
            line = line + (str)(test_X[i, window_size - 1,k]) + ';'
        for c in range(0,predicted.shape[2]):
            line = line + (str)(predicted[i, predicted.shape[1]-1, c]) + ';'
        predictionsFile.write(line + '\n')
else:
    for i in range(0,test_X.shape[0]):
        line = ''
        line = line + (str)(test_X[i,window_size - 1]) + ';'
        for c in range(0,predicted.shape[1]):
            line = line + (str)(predicted[i,c]) + ';'
        predictionsFile.write(line + '\n')

predictionsFile.close()
log("> время создания и записи тестового прогноза  : " + getTime()) 
log("__________________________________")    
log("______________END________________")     
  

RESPONSE = "{RESPONSE:{"
RESPONSE = RESPONSE + "response:{value:скрипт " + prediction_algorithm_name + " успешно завершён"
RESPONSE = RESPONSE + "}}}"
log(RESPONSE)
sys.stderr.close()