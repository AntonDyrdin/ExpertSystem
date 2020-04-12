using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace Экспертная_система
{
    class ReturnToAvarage : Algorithm
    {
        public ReturnToAvarage(MainForm form1, string name) : base(form1, name)
        {
            this.name = "ReturnToAvarage";
            fillFilePaths();


        }

        public override double getPrediction(string[] input)
        {
            return 0;
        }

        public override void runGetPredictionScript() { }
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
            return this;
        }
        public override void Save()
        {
            File.WriteAllText(h.getValueByName("json_file_path"), h.toJSON(0), System.Text.Encoding.Default);
        }
    }
}
