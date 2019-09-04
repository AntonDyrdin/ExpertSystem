using System.IO;

namespace Экспертная_система
{
    public class FlexNN : Algorithm
    {
        public FlexNN(MainForm form1, string name) : base(form1, name)
        {
            this.name = "FlexNN";
            fillFilePaths();



           

            int window_size = 30;


            //     int _7stLayer = h.addByParentId(NNscructNodeId, "name:layer1,value:Dense");
            //     h.addByParentId(_7stLayer, "neurons_count:2");
            //     h.addByParentId(_7stLayer, "activation:softmax");

            addLayer("LSTM", new parameter[] {
                new parameter("neurons_count", 2, 100, 1, 9),
                new parameter("activation", "sigmoid", "sigmoid,linear,relu,softmax")
            });

            addLayer("LSTM", new parameter[] {
                new parameter("neurons_count", 2, 100, 1, 9),
                new parameter("activation", "sigmoid", "sigmoid,linear,relu,softmax")
            });

            addLayer("Dense", new parameter[] {
                new parameter("neurons_count","2"),
                 new parameter("activation","softmax")
            });

            /*  int _1stLayer = h.addByParentId(NNscructNodeId, "name:layer1,value:LSTM");
             h.addVariable(_1stLayer, "neurons_count", 2, 100, 1, 9);
             h.addVariable(_1stLayer, "activation", "sigmoid", "sigmoid,linear");

            int _2stLayer = h.addByParentId(NNscructNodeId, "name:layer2,value:LSTM");
            h.addVariable(_2stLayer, "neurons_count", 2, 100, 1, 9);
            h.addVariable(_2stLayer, "activation", "sigmoid", "sigmoid,linear");


            int _3stLayer = h.addByParentId(NNscructNodeId, "name:layer3,value:Dense");
             h.addByParentId(_3stLayer, "neurons_count:2");
            h.addByParentId(_3stLayer, "activation:softmax");*/

            //////////////////////
            //ПАРАМЕТРЫ ОБУЧЕНИЯ//
            //////////////////////
            h.addVariable(0, "number_of_epochs", 1, 150, 1, 30);
            h.add("split_point:0.7");
            h.addVariable(0, "batch_size", 10, 300, 1, 43);
            //binary_crossentropy mean_squared_error
            h.add("name:loss,value:binary_crossentropy");
            //  h.add("name:optimizer,value:adam");
            h.addVariable(0, "optimizer", "adam", "adam,rmsprop");
            h.addVariable(0, "learning_rate", 0.0001, 0.2, 0.0001, 0.08);
            h.addVariable(0, "window_size", 2, 12, 1, 2);
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
