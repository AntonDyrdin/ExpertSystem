using System.IO;
namespace Экспертная_система
{
    public class LSTM_1 : Algorithm
    {

        public LSTM_1(Form1 form1, string name) : base(form1, name)
        {


            h.add("predicted_column_index:2");
            h.add("drop_columns:<TIME>;<TICKER>;<PER>;<DATE>;<VOL>");


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
            h.add("split_point:0.8");
            h.add("batch_size:200");
            h.add("name:loss,value:mean_squared_error");
            h.add("name:optimizer,value:adam");
            h.add("window_size:10");
        }
        public override string Save(string path)
        {
            h.getNodeByName("json_file_path")[0].setValue(path + "json.txt");
            Directory.CreateDirectory(path);
            File.WriteAllText(h.getValueByName("json_file_path"), h.toJSON(0));
            h.getNodeByName("weights_file_path")[0].setValue(path + "weights.h5");
            /* try
             {*/
            File.Copy(mainFolder + "weights.h5", h.getValueByName("weights_file_path"), true);/* }
            catch { }*/
            return path;
        }
    }
}
