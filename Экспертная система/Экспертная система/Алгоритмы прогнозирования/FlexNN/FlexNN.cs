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

            int _1stLayer = h.addByParentId(NNscructNodeId, "name:layer1,value:Dense");
            h.addVariable(_1stLayer, "neurons_count", 2, 10, 1, 10);
            h.addVariable(_1stLayer, "activation", "sigmoid", "sigmoid,linear");

            int _2stLayer = h.addByParentId(NNscructNodeId, "name:layer2,value:Dropout");
            h.addVariable(_2stLayer, "dropout", 0.01, 0.8, 0.01, 0.1);

            int _3stLayer = h.addByParentId(NNscructNodeId, "name:layer3,value:Dense");
            h.addByParentId(_3stLayer, "neurons_count:2");
            h.addVariable(_3stLayer, "activation", "sigmoid", "sigmoid,linear");

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
