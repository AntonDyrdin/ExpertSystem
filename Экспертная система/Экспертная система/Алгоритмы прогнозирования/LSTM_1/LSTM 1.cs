using System.IO;
namespace Экспертная_система
{
    public class LSTM_1 : Algorithm
    {

        public LSTM_1(MainForm form1, string name) : base(form1, name)
        {
            System.Threading.Thread.Sleep(20);
            this.name = "LSTM_1";
            fillFilePaths();
            ///////////////////////
            //СТРУКТУРА НЕЙРОСЕТИ//
            ///////////////////////
            int NNscructNodeId = h.add("name:NN_struct");

            int _1stLayer = h.addByParentId(NNscructNodeId, "name:layer1,value:LSTM");
            h.addVariable(_1stLayer, "neurons_count", 2, 20, 1, 9);
            h.addVariable(_1stLayer, "activation", "sigmoid", "sigmoid,linear");

            int _2stLayer = h.addByParentId(NNscructNodeId, "name:layer2,value:Dropout");
            h.addVariable(_2stLayer, "dropout", 0.01, 0.8, 0.01, 0.01);

            int _3stLayer = h.addByParentId(NNscructNodeId, "name:layer3,value:Dense");
            h.addVariable(_3stLayer, "neurons_count", 2, 20, 1, 10);
            h.addVariable(_3stLayer, "activation", "sigmoid", "sigmoid,linear");

            int _4stLayer = h.addByParentId(NNscructNodeId, "name:layer4,value:Dropout");
            h.addVariable(_4stLayer, "dropout", 0.01, 0.8, 0.01, 0.01);

            int _5stLayer = h.addByParentId(NNscructNodeId, "name:layer5,value:Dense");
            h.addVariable(_5stLayer, "activation", "sigmoid", "sigmoid,linear");
            //////////////////////
            //ПАРАМЕТРЫ ОБУЧЕНИЯ//
            //////////////////////
            h.addVariable(0, "number_of_epochs", 1, 50, 1, 30);
            h.add("start_point:0");
            h.add("split_point:0.7");
            h.addVariable(0,"batch_size", 10, 300, 1, 10);
            h.addVariable(0, "loss", "mean_squared_error", "mean_absolute_error,mean_squared_error,mean_squared_logarithmic_error,squared_hinge,hinge,categorical_hinge,logcosh,categorical_crossentropy,sparse_categorical_crossentropy,binary_crossentropy,kullback_leibler_divergence,poisson,cosine_proximity");
            h.add("name:optimizer,value:adam");
            h.addVariable(0, "window_size", 2, 120, 1,3);
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
