﻿using System.IO;
namespace Экспертная_система
{
    public class LSTM_1 : Algorithm
    {

        public LSTM_1(Form1 form1, string name) : base(form1, name)
        {
            System.Threading.Thread.Sleep(20);
            this.name = "LSTM_1";
            fillFilePaths();
            ///////////////////////
            //СТРУКТУРА НЕЙРОСЕТИ//
            ///////////////////////
            int NNscructNodeId = h.add("name:NN_sctruct");

            int _1stLayer = h.addByParentId(NNscructNodeId, "name:layer1,value:LSTM");
            h.addVariable(_1stLayer, "neurons_count", 2, 10, 1, new System.Random().Next(2, 10));
            h.addVariable(_1stLayer, "activation", "sigmoid", "sigmoid");

            int _2stLayer = h.addByParentId(NNscructNodeId, "name:layer2,value:Dense");
            h.addVariable(_2stLayer, "neurons_count", 2, 10, 1, new System.Random().Next(2, 10));
            h.addVariable(_2stLayer, "activation", "sigmoid", "sigmoid");

            int _3stLayer = h.addByParentId(NNscructNodeId, "name:layer3,value:Dense");
            h.addByParentId(_3stLayer, "neurons_count:1");
            h.addVariable(_3stLayer, "activation", "sigmoid", "sigmoid,linear");
            //////////////////////
            //ПАРАМЕТРЫ ОБУЧЕНИЯ//
            //////////////////////
            h.add("number_of_epochs:1");
            h.add("start_point:0.3");
            h.add("split_point:0.8");
            h.add("batch_size:" + new System.Random().Next(3, 300).ToString());
            h.add("name:loss,value:mean_squared_error");
            h.add("name:optimizer,value:adam");
            h.addVariable(0, "window_size", 2, 30, 1, new System.Random().Next(2, 30));
        }
        public override void Open(Hyperparameters h)
        {
            this.h = h;
            modelName = getValueByName("model_name");
        }
        public override void Save()
        {
            File.WriteAllText(h.getValueByName("json_file_path"), h.toJSON(0), System.Text.Encoding.Default);
        }
    }
}
