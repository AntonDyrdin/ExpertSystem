using System.IO;
namespace Экспертная_система
{
    public class CNN_1 : Algorithm
    {
        public int window_size;

        public CNN_1(Form1 form1, string name) : base(form1, name)
        {
            System.Threading.Thread.Sleep(20);
            this.name = "CNN_1";
            window_size = new System.Random().Next(30, 50);
            fillFilePaths();
            ///////////////////////
            //СТРУКТУРА НЕЙРОСЕТИ//
            ///////////////////////
            int NNscructNodeId = h.add("name:NN_sctruct");

            int _1stLayer = h.addByParentId(NNscructNodeId, "name:layer1,value:Conv1D");
            h.addByParentId(_1stLayer, "neurons_count:" + window_size.ToString());
            h.addVariable(_1stLayer, "kernel_size", 2, window_size/2,1,3);

            int _2stLayer = h.addByParentId(NNscructNodeId, "name:layer2,value:Conv1D");
            h.addByParentId(_2stLayer, "neurons_count:" + window_size.ToString());
            h.addVariable(_2stLayer, "kernel_size", 2, window_size / 2, 1, 3);

            int _3stLayer = h.addByParentId(NNscructNodeId, "name:layer3,value:MaxPooling1D");
            h.addByParentId(_3stLayer, "MaxPooling1D:3");

            int _4stLayer = h.addByParentId(NNscructNodeId, "name:layer4,value:Conv1D");
            h.addByParentId(_4stLayer, "neurons_count:"+(window_size*2).ToString());
            h.addVariable(_4stLayer, "kernel_size", 2, window_size , 1, 3);

            int _5stLayer = h.addByParentId(NNscructNodeId, "name:layer5,value:Conv1D");
            h.addByParentId(_5stLayer, "neurons_count:" + (window_size * 2).ToString());
            h.addVariable(_5stLayer, "kernel_size", 2, window_size , 1, 3);

            int _6stLayer = h.addByParentId(NNscructNodeId, "name:layer6,value:Dropout");
            h.addVariable(_6stLayer, "dropout", 0.01, 0.8, 0.01, 0.1);

            int _7stLayer = h.addByParentId(NNscructNodeId, "name:layer7,value:Dense");
            h.addVariable(_7stLayer, "activation", "sigmoid", "sigmoid,linear");

            //////////////////////
            //ПАРАМЕТРЫ ОБУЧЕНИЯ//
            //////////////////////
            h.addVariable(0, "number_of_epochs", 1, 20, 1, 10);
            h.add("start_point:0");
            h.add("split_point:0.9");
            h.addVariable(0, "batch_size", 10, 300, 1, 43);
            h.add("name:loss,value:mean_squared_error");
            //  h.add("name:optimizer,value:adam");
            h.addVariable(0, "optimizer", "adam", "adam,rmsprop");
            h.addVariable(0, "window_size", 2, 120, 1, 102);
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