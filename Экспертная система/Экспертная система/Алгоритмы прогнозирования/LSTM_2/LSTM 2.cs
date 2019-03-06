using System.IO;
namespace Экспертная_система
{
    public class LSTM_2 : Algorithm
    {

        public LSTM_2(Form1 form1, string name) : base(form1, name)
        {
            System.Threading.Thread.Sleep(20);
            this.name = "LSTM_2";
            fillFilePaths();
            ///////////////////////
            //СТРУКТУРА НЕЙРОСЕТИ (https://www.youtube.com/watch?v=ftMq5ps503w)
            ///////////////////////
            int NNscructNodeId = h.add("name:NN_sctruct");
            h.addByParentId(NNscructNodeId, "name:layer1,value:LSTM,neurons_count:10");
            h.addByParentId(NNscructNodeId, "name:layer2,value:Dropout,Dropout:0.01");
            h.addByParentId(NNscructNodeId, "name:layer3,value:LSTM,neurons_count:" + new System.Random().Next(50, 100).ToString() + ",activation:sigmoid");
            h.addByParentId(NNscructNodeId, "name:layer4,value:Dropout,Dropout:0.2");
            h.addByParentId(NNscructNodeId, "name:layer5,value:Dense,neurons_count:25,activation:sigmoid");
            h.addByParentId(NNscructNodeId, "name:layer6,value:Dense,neurons_count:1,activation:linear");
            //////////////////////
            //ПАРАМЕТРЫ ОБУЧЕНИЯ//
            //////////////////////
            h.add("number_of_epochs:10");
            h.add("start_point:0.3");
            h.add("split_point:0.8");
            h.add("batch_size:" + new System.Random().Next(300, 512).ToString());
            h.add("name:loss,value:mse");
            h.add("name:optimizer,value:rmsprop");
            h.add("window_size:" + new System.Random().Next(2, 30).ToString());
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