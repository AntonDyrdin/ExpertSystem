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
        print('\n')
    def createParser():
        parser = argparse.ArgumentParser()
        #parser.add_argument('--jsonFile', type=str,
        #default='D:\Anton\Desktop\MAIN\json.txt')
        parser.add_argument('--jsonFile', type=str, default='C:\\Users\\anton\\Рабочий стол\\ExpertSystem\\json.txt')
        return parser

    parser = createParser()
    args = parser.parse_args()
    log(args)
    jsonFile = open(args.jsonFile, 'r')
    jsontext = jsonFile.read()
    jsonFile.close()
    print(jsontext)
    h = json.loads(jsontext)
    print(json.dumps(h,indent=12,ensure_ascii=False))  
    log(h)


    ################################
    inputFile = open(h["baseNode"]["inputFile"]["value"])

    allLines = inputFile.readlines()
    dataset = numpy.zeros((len(allLines) - 1, len(allLines[0].split(';'))),dtype=float)
    window_size = (int)(h["baseNode"]["window_size"]["value"])
    for i in range(1,len(allLines)):
        for j in range(0,len(allLines[i].split(';'))):
            try:      
                 dataset[i - 1,j] = allLines[i].split(';')[j]
            except:
                 i = i
               #print('cant convert ' + allLines[i].split(';')[j] + ' to
               #float')

    Dataset_X = numpy.zeros((dataset.shape[0] - window_size - 1, window_size,dataset.shape[1]), dtype=float)
    Dataset_Y = numpy.zeros(dataset.shape[0] - window_size - 1, dtype=float)
    predicted_column_index = (int)(h["baseNode"]["predicted_column_index"]["value"])
    for i in range(0,dataset.shape[0] - window_size - 1):
        for j in range(0,window_size):
            for k in range(0,dataset.shape[1]):
                Dataset_X[i,j,k] = dataset[i + j][k]
                #вектор Y представляет собой прогнозируемое значение на шаге
                #ряда i+1
        Dataset_Y[i] = dataset[i + window_size,predicted_column_index]
    train_start_point=0
    split_point=0.9
    train_X = Dataset_X[train_start_point:round(Dataset_X.shape[0] * (1 - split_point)), :,:]
    test_X = Dataset_X[round(Dataset_X.shape[0] * (1 - split_point)):, :,:]
    train_y = Dataset_Y[train_start_point:round(Dataset_Y.shape[0] * (1 - split_point)):]
    test_y = Dataset_Y[round(Dataset_Y.shape[0] * (1 - split_point)):]
    log(y.shape)

    log(train_X)
    log(test_X)
    log(train_y)
    log(test_y)

    model = Sequential()         

    model.add(LSTM(layer_specification_array[i], input_shape=(train_X.shape[1], train_X.shape[2])))
    model.add(LSTM(layer_specification_array[i], input_shape=(train_X.shape[1], train_X.shape[2]),return_sequences=True))
    model.add(LSTM(layer_specification_array[i]))
    model.add(LSTM(layer_specification_array[i],return_sequences=True))
    model.add(Dense(layer_specification_array[i],activation='sigmoid'))
    model.add(Dropout(layer_specification_array[i]))
                                                                  
    log("компиляция НС")
        
    model.compile(loss=loss, optimizer=optimizer,metrics=['accuracy'])
    log("НС скомпилированна")

    
    log("обучение НС")
        
    history = model.fit(train_X, train_y, epochs=number_of_epochs, batch_size=batch_size, validation_data=(test_X, test_y), verbose=2, shuffle=False) 
    
    log("построение графиков")
    if Debug_mode == 1:    
        pyplot.plot(history.history['loss'], label='train')
        pyplot.plot(history.history['val_loss'], label='test')
        pyplot.legend()
        pyplot.show()
    if Debug_mode == 1:    
        pyplot.plot(history.history['acc'], label='acc')
        pyplot.plot(history.history['val_acc'], label='val_acc')
        pyplot.legend()
        pyplot.show()
    if namespace.save_folder != "none":
        #model.save_weights(save_folder+'\\' +prediction_algorithm_name +
        #'_weights.h5')
        save_path = namespace.save_folder + u'\\' + prediction_algorithm_name + ".h5"
        log("сохранение модели: " + save_path)

        # no such file or directory -> парсер аргументов командной строки
        # делает все символы СТРОЧНЫМИ
        model.save(save_path)
        log("..сохранено!")
      
    if Debug_mode == 1:
        log("start parsing") 
        predicted = model.predict(test_X)
        log(test_X[:,0,0])
        log(predicted[:,0])



    print("successfully_trained")          
except ValueError as e:
    print("EXCEPTION", e)