using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace Экспертная_система
{
    class Autoencoder : Algorithm
    {
        public Autoencoder(MainForm form1, string name) : base(form1, name)
        {
            this.name = "Autoencoder";
            fillFilePaths();

            addLayer("LSTM", new parameter[] {
                        new parameter("neurons_count",  90, 150, 5,134),
                        new parameter("activation", "sigmoid")
                    });

            addLayer("Dense", new parameter[] {
                        new parameter("neurons_count", 2, 100, 5,25),
                          new parameter("activation","sigmoid")
            });

            addLayer("Dense", new parameter[] {
                   new parameter("neurons_count","3"),
                    new parameter("activation","sigmoid")
            });

            //////////////////////
            //ПАРАМЕТРЫ ОБУЧЕНИЯ//
            //////////////////////

            h.add("batch_size:100");
            // h.addVariable(0, "batch_size", 10, 100, 10, 50);
            h.add("name:loss,value:mean_squared_error");
            h.add("name:optimizer,value:adam");

        }
        public override Algorithm Open(string jsonPath)
        {
            this.h = new Hyperparameters(File.ReadAllText(jsonPath, System.Text.Encoding.Default), form1);

            var localFolder = Path.GetDirectoryName(jsonPath);

            string jsonFilePath = localFolder + "\\h.json";
            string predictionsFilePath = localFolder + "predictions.txt";
            h.setValueByName("predictions_file_path", predictionsFilePath);
            h.setValueByName("json_file_path", jsonFilePath);
            h.setValueByName("save_folder", localFolder);

            h.setValueByName("model_name", modelName);

            //modelName = getValueByName("model_name");
            return this;
        }
        public override void Save()
        {
            File.WriteAllText(h.getValueByName("json_file_path"), h.toJSON(0), System.Text.Encoding.Default);
        }
    }
}
