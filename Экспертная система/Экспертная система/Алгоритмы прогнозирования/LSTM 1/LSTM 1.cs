namespace Экспертная_система
{
    public class LSTM_1 : Algorithm
    {
        //public Form1 form1;
        public LSTM_1(Form1 form1, string name) : base(form1, name)
        {
            h.add("predicted_column_index:3");
            h.add("drop_column:<DATE>");

            h.add("trainScriptPath:" + form1.pathPrefix + "Экспертная система\\Экспертная система\\Алгоритмы прогнозирования\\" + name + "\\trainScript.py");
            trainScriptPath = h.getValueByName("trainScriptPath");
            jsonFilePath = System.IO.Path.GetDirectoryName(trainScriptPath) + "\\json.txt";
            predictionsFilePath = System.IO.Path.GetDirectoryName(trainScriptPath) + "\\predictions.txt";
            h.add("predictionsFilePath", predictionsFilePath);

            h.add("save_folder:none");
            ///////////////////////
            //СТРУКТУРА НЕЙРОСЕТИ//
            ///////////////////////
            int NNscructNodeId = h.add("name:NN_sctruct");
            h.addByParentId(NNscructNodeId, "name:layer1,value:LSTM,neurons_count:3");
            h.addByParentId(NNscructNodeId, "name:layer2,value:Dense,neurons_count:1,activation:sigmoid");
            h.addByParentId(NNscructNodeId, "name:layer3,value:Dense,neurons_count:1,activation:sigmoid");
            //////////////////////
            //ПАРАМЕТРЫ ОБУЧЕНИЯ//
            //////////////////////
            h.add("number_of_epochs:1");
            h.add("split_point:0.95");
            h.add("batch_size:200");
            h.add("name:loss,value:mean_squared_error");
            h.add("name:optimizer,value:adam");
            h.add("window_size:40");
        }
        public string get_prediction()
        {
            return "ошибка: метод не реализован";
        }

    }
}
