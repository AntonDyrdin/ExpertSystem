using System.IO;
namespace Экспертная_система
{
    public class ANN_1 : Algorithm
    {
        // В данной ANN количество нейронов в первом полносвязном слое равно размеру окна 
        public int window_size;


        public ANN_1(MainForm form1, string name) : base(form1, name)
        {
            System.Threading.Thread.Sleep(20);
            window_size = new System.Random().Next(2, 30);
            this.name = "ANN_1";
            fillFilePaths();
            ///////////////////////
            //СТРУКТУРА НЕЙРОСЕТИ//
            ///////////////////////
            int NNscructNodeId = h.add("name:NN_struct");
            int _1stLayer = h.addByParentId(NNscructNodeId, "name:layer1,value:Dense");
            h.addByParentId(_1stLayer, "neurons_count:"+ window_size.ToString());
            h.addVariable(_1stLayer, "activation", "sigmoid", "sigmoid,linear");

            int _2stLayer = h.addByParentId(NNscructNodeId, "name:layer2,value:Dense");
            h.addVariable(_2stLayer, "neurons_count", 2, 100, 1, 10);
            h.addVariable(_2stLayer, "activation", "sigmoid", "sigmoid,linear");

            int _3stLayer = h.addByParentId(NNscructNodeId, "name:layer3,value:Dense");
            h.addByParentId(_3stLayer, "neurons_count:1");
            h.addVariable(_3stLayer, "activation", "sigmoid", "sigmoid,linear");

            //////////////////////
            //ПАРАМЕТРЫ ОБУЧЕНИЯ//
            //////////////////////
              h.addVariable(0, "number_of_epochs", 1, 10, 1, 1);
            h.add("start_point:0");
            h.add("split_point:0.7");
            h.addVariable(0,"batch_size", 10, 300, 1, 5);
            h.add("name:loss,value:mean_squared_error");
            h.add("name:optimizer,value:adam");
            h.addVariable(0, "window_size", 2, 120, 1, window_size);
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