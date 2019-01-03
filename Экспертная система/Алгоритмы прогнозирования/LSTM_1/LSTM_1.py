try:
    import argparse
    import numpy
    from pandas import read_csv
    from datetime import datetime
    from matplotlib import pyplot
    from pandas import DataFrame
    from pandas import concat
    from keras.models import Sequential
    from keras.layers import Dense
    from keras.layers import LSTM
    from keras.layers import Dropout
    prediction_algorithm_name = 'LSTM_4'
    def log(s):
        if Debug_mode == 1:
            print(s)
            print('\n')
    def createParser():
        parser = argparse.ArgumentParser()
        parser.add_argument('--CLOSE_column_index', type=int, default=2)
        parser.add_argument('--input_file', default="D:\\main\\data_base\\algorithm_check\\EURRUB.txt")
        parser.add_argument('--number_of_epochs', type=int, default=2)
        parser.add_argument('--period', default='day')
        #split_point - доля тестовой выборки от всего датасета
        parser.add_argument('--split_point', default=0.1)
        parser.add_argument('--loss', default="mae")
        parser.add_argument('--optimizer', default="adam")
        parser.add_argument('--batch_size', type=int, default=8)
        parser.add_argument('--number_of_inputs', default=1)
        parser.add_argument('--features', type=int,default=4)
        parser.add_argument('--window_size', type=int,default=30)
        parser.add_argument('--debug_mode', type=int,default=1)
        parser.add_argument('--types_of_layers', default="LSTM,Dropout,Dense,Dropout,Dense")
        parser.add_argument('--layer_specification_array', default="32,0.5,10,0.5,1")
        parser.add_argument('--save_folder', default='d:\main\prediction_algorithms\lstm_2_2\model\lstm_2_2.h5')
        parser.add_argument('--date_column',type=int, default='0')
        return parser



    train_start_point = 0

    continue_ = ''


    parser = createParser()
    namespace = parser.parse_args()
    #print (namespace)
    Debug_mode = namespace.debug_mode
    
    log('')
    log("чтение аргументов коммандной строки")
    CLOSE_column_index = namespace.CLOSE_column_index
    number_of_epochs = namespace.number_of_epochs
    period = namespace.period
    string_types_of_layers = namespace.types_of_layers
    types_of_layers = string_types_of_layers.split(',')
    string_layer_specification_array = namespace.layer_specification_array
    layer_specification_array = string_layer_specification_array.split(',')
    split_point = float(namespace.split_point)
    loss = namespace.loss
    optimizer = namespace.optimizer
    batch_size = namespace.batch_size
    number_of_inputs = namespace.number_of_inputs
    window = namespace.window_size
    features = namespace.features


    def to_3D(data, window_size,features,CLOSE_column_index):
        #time_series=DataFrame(data)
        next_price = 0
        ds_X = numpy.zeros((len(data) - window_size - 1, window_size,features), dtype=float)
        ds_Y = numpy.zeros(len(data) - window_size - 1, dtype=float)

       # print(ds_X.shape)

        #print(len(data)-window_size-1)
        for i in range(0,len(data) - window_size - 1):
            for j in range(0,window_size):
                for k in range(0,features):
                    #print(data[i+j].shape)
                    ds_X[i,j,k] = data[i + j][k]

            p2 = data[i + window_size][CLOSE_column_index]
            p1 = ds_X[i,window_size - 1,CLOSE_column_index]
            if p2 > 1:
                ds_Y[i] = 1   
            else:
                ds_Y[i] = 0
        return ds_X,ds_Y







    def parse_date_AND_time(x):
        return datetime.strptime(x, "%Y%m%d %H:%M:%S")
    def parse_date(x):
        return datetime.strptime(x, "%d/%m/%y")
    #f =
    #open(u'D:\\main\\data_base\\database_Long_files_1_'+period+'\\'+symbol_name+'.txt')
    log("чтение .csv файла")
    f = open(namespace.input_file)
    if period == "day":
        dataset = read_csv(f,index_col=namespace.date_column,date_parser=parse_date)
    if period == "minute":
        dataset = read_csv(f,index_col=namespace.date_column,date_parser=parse_date_AND_time)
       
   # dataset=dataset.drop('<TICKER>',1)
    #dataset=dataset.drop('<TIME>',1)
    #dataset=dataset.drop('<PER>',1)
    #dataset=dataset.drop('<VOL>',1)  

    #dataset=dataset.drop('<HIGH>',1)
    #dataset=dataset.drop('<LOW>',1)
    #dataset=dataset.drop('<CLOSE>',1)


    inc3=0
    for  columnName in dataset:
        if columnName=='<CLOSE>':
            CLOSE_column_index=inc3
        inc3=inc3+1

    values = dataset.values
    log(values)
    inc = 0
    log("нормализация данных")
    scaled = list()
    for  value in values:
   
        if inc == 0:
            tempvalue = value
        else:
            if  tempvalue.all != 0:
                for i in range(0,len(value)):
                    tempvalue[i] = value[i] / tempvalue[i]
            
                scaled.append(tempvalue)
        
            tempvalue = value
        inc = inc + 1
   

    values = scaled



    log("X,y= to_3D(values, window, features,namespace.CLOSE_column_index)")
    ################################
    ################################
    #data, window_size,features,CLOSE_column_index)
    ################################
    X,y = to_3D(values, window, features,CLOSE_column_index)
    ################################
    ################################



    log("разделение на обучающую и тренировочную выборку")
    train_X = X[train_start_point:round(X.shape[0] * (1 - split_point)), :,:]
    test_X = X[round(X.shape[0] * (1 - split_point)):, :,:]
    train_y = y[train_start_point:round(y.shape[0] * (1 - split_point)):]
    log(split_point)
    test_y = y[round(y.shape[0] * (1 - split_point)):]
    log(y.shape)

    log(train_X)
    log(test_X)
    log(train_y)
    log(test_y)

    log("определение параметров НС")
    model = Sequential()
    count = 0

    inc = 0
    while inc < len(layer_specification_array):
        if layer_specification_array[inc].find('.') == -1:
            layer_specification_array[inc] = int(layer_specification_array[inc])
        else:
            layer_specification_array[inc] = float(layer_specification_array[inc])
        inc = inc + 1
    inc = 0


    for i in range(0,len(types_of_layers)):
        ############
        ####LSTM####
        ############
        if types_of_layers[i] == "LSTM":
             if i == 0:
                 if types_of_layers[i + 1] != "LSTM":
                     model.add(LSTM(layer_specification_array[i], input_shape=(train_X.shape[1], train_X.shape[2])))
                     log("add LSTM layer with " + str(layer_specification_array[i]) + " neurons return_sequences=False")
                 else:
                    model.add(LSTM(layer_specification_array[i], input_shape=(train_X.shape[1], train_X.shape[2]),return_sequences=True))
                    log("add LSTM layer with " + str(layer_specification_array[i]) + " neurons return_sequences=True")
       
             else:
                if types_of_layers[i + 1] != "LSTM":
                    model.add(LSTM(layer_specification_array[i]))
                    log("add LSTM layer with " + str(layer_specification_array[i]) + " neurons return_sequences=False")
                else:   
                   model.add(LSTM(layer_specification_array[i],return_sequences=True))
                   log("add LSTM layer with " + str(layer_specification_array[i]) + " neurons return_sequences=True")

        ############
        ####Dense###
        ############
        if types_of_layers[i] == "Dense":
          model.add(Dense(layer_specification_array[i],activation='sigmoid'))
          log("add Dense layer with " + str(layer_specification_array[i]) + " neurons")

        ################
        ####Dropout####
        ################
        if types_of_layers[i] == "Dropout":
          model.add(Dropout(layer_specification_array[i]))
          log("add Dropout layer" )



    
    log("компиляция НС")
        
    model.compile(loss=loss, optimizer=optimizer,metrics=['accuracy'])
    log("model compile")

    
    log("обучение НС")
        
    history = model.fit(train_X, train_y, epochs=number_of_epochs, batch_size=batch_size, validation_data=(test_X, test_y), verbose=2, shuffle=False) 
    log("model.fit")

    
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
    print("EXEPTION", e)