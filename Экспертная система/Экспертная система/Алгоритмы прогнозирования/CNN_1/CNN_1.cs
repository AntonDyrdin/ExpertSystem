using System.IO;
namespace Экспертная_система
{
    public class CNN_1 : Algorithm
    {
        public int window_size;

        public CNN_1(MainForm form1, string name) : base(form1, name)
        {
            System.Threading.Thread.Sleep(20);
            this.name = "CNN_1";
            window_size = 30;
            fillFilePaths();
            ///////////////////////
            //СТРУКТУРА НЕЙРОСЕТИ//
            ///////////////////////
            int NNscructNodeId = h.add("name:NN_sctruct");

            /*  int _1stLayer = h.addByParentId(NNscructNodeId, "name:layer1,value:Conv1D");
              h.addByParentId(_1stLayer, "neurons_count:" + window_size.ToString());
              h.addVariable(_1stLayer, "kernel_size", 3, 3, 1, 3);

              int _2stLayer = h.addByParentId(NNscructNodeId, "name:layer2,value:Conv1D");
              h.addByParentId(_2stLayer, "neurons_count:" + window_size.ToString());
              h.addVariable(_2stLayer, "kernel_size", 3, 3, 1, 3);

              int _3stLayer = h.addByParentId(NNscructNodeId, "name:layer3,value:MaxPooling1D");
              h.addByParentId(_3stLayer, "MaxPooling1D:3");

              int _4stLayer = h.addByParentId(NNscructNodeId, "name:layer4,value:Conv1D");
              h.addByParentId(_4stLayer, "neurons_count:"+(window_size*2).ToString());
              h.addVariable(_4stLayer, "kernel_size", 3, 3, 1, 3);

              int _5stLayer = h.addByParentId(NNscructNodeId, "name:layer5,value:Conv1D");
              h.addByParentId(_5stLayer, "neurons_count:" + (window_size * 2).ToString());
              h.addVariable(_5stLayer, "kernel_size", 3, 3, 1, 3);

              int _6stLayer = h.addByParentId(NNscructNodeId, "name:layer6,value:Dropout");
              h.addVariable(_6stLayer, "dropout", 0.01, 0.8, 0.01, 0.1);

              int _7stLayer = h.addByParentId(NNscructNodeId, "name:layer7,value:Dense");
              h.addVariable(_7stLayer, "activation", "sigmoid", "sigmoid,linear");*/

            int _1stLayer = h.addByParentId(NNscructNodeId, "name:layer1,value:Conv1D");
            h.addByParentId(_1stLayer, "neurons_count:" + window_size.ToString());
            h.addByParentId(_1stLayer, "kernel_size:3");

            int _2stLayer = h.addByParentId(NNscructNodeId, "name:layer2,value:Conv1D");
            h.addByParentId(_2stLayer, "neurons_count:" + window_size.ToString());
            h.addByParentId(_2stLayer, "kernel_size:3");

            int _3stLayer = h.addByParentId(NNscructNodeId, "name:layer3,value:MaxPooling1D");
            h.addByParentId(_3stLayer, "MaxPooling1D:3");

            int _4stLayer = h.addByParentId(NNscructNodeId, "name:layer4,value:Conv1D");
            h.addByParentId(_4stLayer, "neurons_count:" + (window_size * 2).ToString());
            h.addByParentId(_4stLayer, "kernel_size:3");

            int _5stLayer = h.addByParentId(NNscructNodeId, "name:layer5,value:Conv1D");
            h.addByParentId(_5stLayer, "neurons_count:" + (window_size * 2).ToString());
            h.addByParentId(_5stLayer, "kernel_size:3");

            int _6stLayer = h.addByParentId(NNscructNodeId, "name:layer6,value:Dropout");
            h.addByParentId(_6stLayer, "dropout:0.1");

            int _7stLayer = h.addByParentId(NNscructNodeId, "name:layer7,value:Dense");
            h.addByParentId(_7stLayer, "activation:sigmoid");
            //////////////////////
            //ПАРАМЕТРЫ ОБУЧЕНИЯ//
            //////////////////////
            h.addVariable(0, "number_of_epochs", 1, 30, 1, 13);
            h.add("start_point:0");
            h.add("split_point:0.7");
            h.addVariable(0, "batch_size", 10, 300, 1, 220);
            h.addVariable(0,"loss", "mean_squared_error", "mean_absolute_error,mean_squared_error,mean_squared_logarithmic_error,squared_hinge,hinge,categorical_hinge,logcosh,categorical_crossentropy,sparse_categorical_crossentropy,binary_crossentropy,kullback_leibler_divergence,poisson,cosine_proximity");
            h.add("name:optimizer,value:adam");
            //h.addVariable(0, "optimizer", "adam", "adam,rmsprop");
            h.addVariable(0, "window_size", 10, 120, 1, window_size);
        }
        public override void Open(string jsonPath)
        {
            this.h = new Hyperparameters(File.ReadAllText(jsonPath, System.Text.Encoding.Default), form1);
            modelName = getValueByName("model_name");
        }
        public override void Save()
        {
            File.WriteAllText(h.getValueByName("json_file_path"), h.toJSON(0), System.Text.Encoding.Default);
        }
    }
}