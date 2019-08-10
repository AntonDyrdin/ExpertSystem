using System.IO;

namespace Экспертная_система
{
    public class FlexNN : Algorithm
    {
        public FlexNN(MainForm form1, string name) : base(form1, name)
        {
            this.name = "FlexNN";
            fillFilePaths();
            int NNscructNodeId = h.add("name:NN_struct");

            int window_size = 30;

            int _1stLayer = h.addByParentId(NNscructNodeId, "name:layer1,value:Conv1D");
            h.addVariable(_1stLayer, "neurons_count",1,128,1,16);
            h.addVariable(_1stLayer, "kernel_size", 3, 3, 1, 3);

            int _2stLayer = h.addByParentId(NNscructNodeId, "name:layer2,value:Conv1D");
            h.addVariable(_2stLayer, "neurons_count", 1, 128, 1, 16);
            h.addVariable(_2stLayer, "kernel_size", 3, 3, 1, 3);

            int _3stLayer = h.addByParentId(NNscructNodeId, "name:layer3,value:MaxPooling1D");
            h.addByParentId(_3stLayer, "pool_size:3");

            int _4stLayer = h.addByParentId(NNscructNodeId, "name:layer4,value:Conv1D");
            h.addVariable(_4stLayer, "neurons_count", 1, 128, 1, 32);
            h.addVariable(_4stLayer, "kernel_size", 3, 3, 1, 3);

            int _5stLayer = h.addByParentId(NNscructNodeId, "name:layer5,value:Conv1D");
            h.addVariable(_5stLayer, "neurons_count", 1, 128, 1, 32);
            h.addVariable(_5stLayer, "kernel_size", 3, 3, 1, 3);

            int _6stLayer = h.addByParentId(NNscructNodeId, "name:layer6,value:Flatten");

            int _7stLayer = h.addByParentId(NNscructNodeId, "name:layer7,value:Dense");
            h.addByParentId(_7stLayer, "neurons_count:2");
            h.addByParentId(_7stLayer, "activation:softmax");


            /*  int _1stLayer = h.addByParentId(NNscructNodeId, "name:layer1,value:Dense");
             h.addVariable(_1stLayer, "neurons_count", 2, 10, 1, 10);
             h.addVariable(_1stLayer, "activation", "sigmoid", "sigmoid,linear");

             int _2stLayer = h.addByParentId(NNscructNodeId, "name:layer2,value:Dropout");
             h.addVariable(_2stLayer, "dropout", 0.01, 0.8, 0.01, 0.1);

             int _3stLayer = h.addByParentId(NNscructNodeId, "name:layer3,value:Dense");
             h.addByParentId(_3stLayer, "neurons_count:2");
            h.addByParentId(_3stLayer, "activation:softmax");
             */
            //////////////////////
            //ПАРАМЕТРЫ ОБУЧЕНИЯ//
            //////////////////////
            h.addVariable(0, "number_of_epochs", 1, 20, 1, 1);
            h.add("split_point:0.7");
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
