using System.IO;

namespace Экспертная_система
{
    public class FlexNN : Algorithm
    {
        public FlexNN(MainForm form1, string name) : base(form1, name)
        {
            this.name = "FlexNN";
            fillFilePaths();

            addLayer("LSTM", new parameter[] {
                new parameter("neurons_count", 2, 100, 5, 9),
                new parameter("activation", "sigmoid", "sigmoid,linear,relu,softmax")
            });

            addLayer("LSTM", new parameter[] {
                new parameter("neurons_count", 2, 100, 5, 20),
                new parameter("activation", "sigmoid", "sigmoid,linear,relu,softmax")
            });

        /*    addLayer("Dropout", new parameter[] {
                new parameter("dropout", 0.01, 0.3, 0.1, 0.2),
            });

            addLayer("Dense", new parameter[] {
                new parameter("neurons_count", 2, 100, 5, 10),
                new parameter("activation", "sigmoid", "sigmoid,linear,relu,softmax")
            });

            addLayer("Dropout", new parameter[] {
                new parameter("dropout", 0.01, 0.3, 0.1, 0.2),
            });*/

            addLayer("Dense", new parameter[] {
                new parameter("neurons_count","2"),
                 new parameter("activation","softmax")
            });

            //////////////////////
            //ПАРАМЕТРЫ ОБУЧЕНИЯ//
            //////////////////////
            h.addVariable(0, "number_of_epochs", 1, 150, 5,20);
            h.add("split_point:0.9");
            h.addVariable(0, "batch_size", 10, 300, 10, 50);
            //binary_crossentropy mean_squared_error
            h.add("name:loss,value:binary_crossentropy");
            //  h.add("name:optimizer,value:adam");
            h.addVariable(0, "optimizer", "adam", "adam,rmsprop");
            h.addVariable(0, "learning_rate", 0.0001, 0.2, 0.05, 0.005);
            h.addVariable(0, "window_size", 2, 20, 2, 10);
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
