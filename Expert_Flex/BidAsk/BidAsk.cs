using System;
using System.IO;

namespace Экспертная_система
{
    public class BidAsk : Algorithm
    {
        public BidAsk(MainForm form1, string name) : base(form1, name)
        {
            this.name = "BidAsk";
            fillFilePaths();

            /*  addLayer("Conv1D", new parameter[] {
                  new parameter("neurons_count", 2, 100, 5, 30),
                  new parameter("kernel_size", "3")
              });
              addLayer("Conv1D", new parameter[] {
                  new parameter("neurons_count", 2, 100, 5, 30),
                  new parameter("kernel_size", "3")
              }); */

            /*  addLayer("MaxPooling1D", new parameter[] {
                    new parameter("pool_size","3"),
              });*/
            /* addLayer("LSTM", new parameter[] {
                   new parameter("neurons_count", 2, 200, 5,250),
                   new parameter("activation", "sigmoid")
               });
             addLayer("LSTM", new parameter[] {
                   new parameter("neurons_count", 2, 200, 5,250),
                   new parameter("activation", "sigmoid")
               });*/


               addLayer("LSTM", new parameter[] {
                   new parameter("neurons_count", 2, 200, 5,100),
                   new parameter("activation", "sigmoid")
               });
            addLayer("LSTM", new parameter[] {
                   new parameter("neurons_count", 2, 200, 5,50),
                   new parameter("activation", "sigmoid")
               });
            addLayer("LSTM", new parameter[] {
                   new parameter("neurons_count", 2, 200, 5,25),
                   new parameter("activation", "sigmoid")
               });

            /*    addLayer("Dense", new parameter[] {
                      new parameter("neurons_count", 2, 100, 5,6),
                        new parameter("activation","relu","relu,linear,sigmoid")
                   });*/
            /*  addLayer("Dropout", new parameter[] {
                     new parameter("dropout","0.2"),
                 });*/
            addLayer("Dense", new parameter[] {
                   new parameter("neurons_count","3"),
                    new parameter("activation","softmax")
               });

            //////////////////////
            //ПАРАМЕТРЫ ОБУЧЕНИЯ//
            //////////////////////
            h.addVariable(0, "number_of_epochs", 1, 50, 1, 10);
            h.add("wait_for_rise:20");
            h.addVariable(0,"split_point", 0.7, 0.95, 0.05, 0.9);
            h.add("batch_size:50");
            // h.addVariable(0, "batch_size", 10, 100, 10, 50);
            h.add("name:loss,value:binary_crossentropy");
            h.add("name:optimizer,value:adam");
            h.addVariable(0, "learning_rate", 0.0001, 0.02, 0.05, 0.001);
            h.addVariable(0, "window_size", 2, 120, 2, 120);
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
