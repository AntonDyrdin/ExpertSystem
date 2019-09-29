using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace Экспертная_система
{
    class Easy : Algorithm
    {
        public Easy(MainForm form1, string name) : base(form1, name)
        {
            this.name = "Easy";
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
                     new parameter("neurons_count", 2, 200, 5,50),
                     new parameter("activation", "sigmoid")
                 });
              addLayer("LSTM", new parameter[] {
                     new parameter("neurons_count", 2, 200, 5,20),
                     new parameter("activation", "sigmoid")
                 });
            addLayer("Dropout", new parameter[] {
                     new parameter("dropout","0.2"),
                 });

            addLayer("Dense", new parameter[] {
                      new parameter("neurons_count", 2, 100, 5,11),
                        new parameter("activation","sigmoid")
                   });
            addLayer("Dense", new parameter[] {
                      new parameter("neurons_count", 2, 100, 5,11),
                        new parameter("activation","sigmoid")
                   });
            /*  addLayer("Dropout", new parameter[] {
                     new parameter("dropout","0.2"),
                 });*/
            addLayer("Dense", new parameter[] {
                   new parameter("neurons_count","1"),
                    new parameter("activation","sigmoid")
               });

            //////////////////////
            //ПАРАМЕТРЫ ОБУЧЕНИЯ//
            //////////////////////
            h.addVariable(0, "number_of_epochs", 1, 150, 1, 50);
            h.addVariable(0, "split_point", 0.7, 0.95, 0.05, 0.74);
            h.add("batch_size:50");
            // h.addVariable(0, "batch_size", 10, 100, 10, 50);
            h.add("name:loss,value:mean_squared_error");
            h.add("name:optimizer,value:adam");
            h.addVariable(0, "learning_rate", 0.0001, 0.02, 0.05, 0.00559);
            h.addVariable(0, "window_size", 2, 20, 2, 30);
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
