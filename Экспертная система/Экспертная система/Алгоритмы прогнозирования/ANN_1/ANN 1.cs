using System.IO;
namespace Экспертная_система
{
    public class ANN_1 : Algorithm
    {
        // В данной ANN количество нейронов в первом полносвязном слое равно размеру окна 
        public int window_size;


        public ANN_1(Form1 form1, string name) : base(form1, name)
        {
<<<<<<< HEAD
            System.Threading.Thread.Sleep(20);
            window_size = new System.Random().Next(2, 20);
=======
>>>>>>> 20f1c5e4909c89b006531b36e42f5f8246b6b222
            this.name = "ANN_1";
            fillFilePaths();
            ///////////////////////
            //СТРУКТУРА НЕЙРОСЕТИ//
            ///////////////////////
            int NNscructNodeId = h.add("name:NN_sctruct");
            h.addByParentId(NNscructNodeId, "name:layer1,value:Dense,neurons_count:" + window_size.ToString());
            h.addByParentId(NNscructNodeId, "name:layer2,value:Dense,neurons_count:6,activation:sigmoid");
            h.addByParentId(NNscructNodeId, "name:layer3,value:Dense,neurons_count:1,activation:sigmoid");
            //////////////////////
            //ПАРАМЕТРЫ ОБУЧЕНИЯ//
            //////////////////////
            h.add("number_of_epochs:1");
            h.add("split_point:0.8");
            h.add("batch_size:200");
            h.add("name:loss,value:mean_squared_error");
            h.add("name:optimizer,value:adam");
            h.add("window_size:" + window_size.ToString());
        }
        public override void Open(Hyperparameters h)
        {
            this.h = h;
            modelName = getValueByName("model_name");
            window_size = System.Convert.ToInt32(getValueByName("window_size"));
        }
        public override void Save()
        {
            File.WriteAllText(h.getValueByName("json_file_path"), h.toJSON(0), System.Text.Encoding.Default);
        }
    }
}
