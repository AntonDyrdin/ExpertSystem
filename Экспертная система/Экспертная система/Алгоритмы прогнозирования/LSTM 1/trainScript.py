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
        parser.add_argument('--jsonFile', type=str, default='D:\Anton\Desktop\MAIN\json.txt')
        return parser

    parser = createParser()
    args = parser.parse_args()
    log(args)
    jsonFile = open(args.jsonFile, 'r')
    jsontext=jsonFile.read()
    jsonFile.close()
    print(jsontext)
    h= json.loads(jsontext)
    print(json.dumps(h,indent=12,ensure_ascii=False))  
    log(h)
    ################################
    ################################
    #data, window_size,features,CLOSE_column_index)
    ################################
    X,y = to_3D(values, window, features,CLOSE_column_index)
    ################################
    ################################



    train_X = X[train_start_point:round(X.shape[0] * (1 - split_point)), :,:]
    test_X = X[round(X.shape[0] * (1 - split_point)):, :,:]
    train_y = y[train_start_point:round(y.shape[0] * (1 - split_point)):]
    test_y = y[round(y.shape[0] * (1 - split_point)):]
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
        #model.save_weights(save_folder+'\\' +prediction_algorithm_name + '_weights.h5')
        save_path=namespace.save_folder  +u'\\' +prediction_algorithm_name  +".h5"
        log("сохранение модели: "+save_path)

        # no such file or directory -> парсер аргументов командной строки делает все символы СТРОЧНЫМИ
        model.save(save_path)
        log("..сохранено!")
      
    if Debug_mode==1:
        log("start parsing") 
        predicted = model.predict(test_X)
        log(test_X[:,0,0])
        log(predicted[:,0])



    print("successfully_trained")          
except ValueError as e:
    print("EXCEPTION", e)