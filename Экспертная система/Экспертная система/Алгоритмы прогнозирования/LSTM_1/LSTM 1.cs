using System.IO;
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
            h.addByParentId(NNscructNodeId, "name:layer1,value:LSTM,neurons_count:"+ new System.Random().Next(1, 10).ToString());
            h.addByParentId(NNscructNodeId, "name:layer2,value:Dense,neurons_count:"+ new System.Random().Next(1, 10).ToString()+",activation:sigmoid");
            h.addByParentId(NNscructNodeId, "name:layer3,value:Dense,neurons_count:1,activation:sigmoid");
            //////////////////////
            //ПАРАМЕТРЫ ОБУЧЕНИЯ//
            //////////////////////
            h.add("number_of_epochs:1");
            h.add("split_point:0."+ new System.Random().Next(6, 95).ToString());
            h.add("batch_size:"+new System.Random().Next(3, 300).ToString());
            h.add("name:loss,value:mean_squared_error");
            h.add("name:optimizer,value:adam");
            h.add("window_size:"+ new System.Random().Next(2, 30).ToString());

            h.add("name:show_train_charts,value:True");
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
