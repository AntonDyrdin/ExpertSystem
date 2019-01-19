try:
    import argparse
    import numpy
    import json
    from pandas import read_csv
    from datetime import datetime
    from matplotlib import pyplot
    from pandas import DataFrame
    from pandas import concat
    from keras.models import Sequential
    from keras.layers import Dense
    from keras.layers import LSTM
    from keras.layers import Dropout
    prediction_algorithm_name = 'LSTM_1'

    def log(s):
        print(s)

    def createParser():
        parser = argparse.ArgumentParser()
        parser.add_argument('--jsonFile',type=str,default='D:\Anton\Desktop\MAIN\json.txt')
       # parser.add_argument('--jsonFile', type=str,default='C:\\Users\\anton\\Рабочий стол\\MAIN\\json.txt')
        return parser
    def h(nodeName):
        return  jsonObj["baseNode"][nodeName]["value"]

    def h2(nodeName1,nodeName2):
        return  jsonObj["baseNode"][nodeName1][nodeName2]["value"]

    def getAttr2(nodeName1,nodeName2,attrName):
        return  jsonObj["baseNode"][nodeName1][nodeName2][attrName]

    def getAttr2int(nodeName1,nodeName2,attrName):
        return  (int)(jsonObj["baseNode"][nodeName1][nodeName2][attrName])

    parser = createParser()
    args = parser.parse_args()
    jsonFile = open(args.jsonFile, 'r')
    jsontext = jsonFile.read()
    jsonFile.close()
    print(jsontext)
    jsonObj = json.loads(jsontext)
    print(json.dumps(jsonObj,indent=12,ensure_ascii=False))  

    inputFile = open(h("inputFile"))

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
                #вектор Y представляет собой прогнозируемое значение
        Dataset_Y[i] = dataset[i + window_size,predicted_column_index]
    train_start_point = 0
    split_point = (float)(h("split_point"))
    train_X = Dataset_X[train_start_point:round(Dataset_X.shape[0] * (split_point)), :,:]
    test_X = Dataset_X[round(Dataset_X.shape[0] * (split_point)):, :,:]
    train_y = Dataset_Y[train_start_point:round(Dataset_Y.shape[0] * (split_point)):]
    test_y = Dataset_Y[round(Dataset_Y.shape[0] * (split_point)):]

    model = Sequential()         

    model.add(LSTM(getAttr2int("NN_sctruct","layer1","neurons_count"), input_shape=(train_X.shape[1], train_X.shape[2])))
    model.add(Dense(getAttr2int("NN_sctruct","layer2","neurons_count"),activation=getAttr2("NN_sctruct","layer2","activation")))
    model.add(Dense(getAttr2int("NN_sctruct","layer3","neurons_count"),activation=getAttr2("NN_sctruct","layer3","activation")))
                                                                  
    log("компиляция НС...")
        
    model.compile(loss=h("loss"), optimizer=h("optimizer"),metrics=['accuracy'])
    log("НС скомпилированна")

    
    log("обучение НС...")
        
    history = model.fit(train_X, train_y, epochs=(int)(h("number_of_epochs")), batch_size=(int)(h("batch_size")), validation_data=(test_X, test_y), verbose=2, shuffle=False) 
    
    if h("save_folder") != "none":
        model.save_weights(save_folder+'\\' +prediction_algorithm_name +
        '_weights.h5')
        save_path = namespace.save_folder + u'\\' + prediction_algorithm_name
        + ".h5"
        log("сохранение модели: " + save_path)

        # no such file or directory -> парсер аргументов командной строки
        # делает все символы СТРОЧНЫМИ
        model.save(save_path)
        log("..сохранено!")
      
    predicted = model.predict(test_X)
    log(predicted.shape)
    predictionsFile = open(h("pathPrefix") + 'predictions.txt', 'w')
    head = ''
    for i in range(0,len(allLines[0].split(';'))):
        head = head + allLines[0].split(';')[i] + ';'
    head = head[0:-1]
    head = head.replace('\n','')
    log(h("pathPrefix") + 'predictions.txt')
    head = head + '(predicted -> )' + allLines[0].split(';')[(int)(h("predicted_column_index"))]
    predictionsFile.write(head + '\n')

    for i in range(0,test_X.shape[0]):
        line = ''
        for k in range(0,test_X.shape[2]): 
            line = line + (str)(test_X[i,window_size - 1,k]) + ';'
        line = line + (str)(predicted[i,0])
        predictionsFile.write(line + '\n')
    predictionsFile.close()

    print("successfully_trained")          
except ValueError as e:
    print("EXCEPTION", e)