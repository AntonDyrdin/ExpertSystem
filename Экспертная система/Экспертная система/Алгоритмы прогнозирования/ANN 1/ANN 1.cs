using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Экспертная_система
{
    public class ANN_1 : Algorithm
    {
        // В данной ANN количество нейронов в первом полносвязном слое равно размеру окна 
        public int window_size=10;
        

        public ANN_1(Form1 form1, string name) : base(form1, name)
        {
            h.add("predicted_column_index:2");
            h.add("drop_columns:<TIME>;<TICKER>;<PER>;<DATE>;<VOL>");


            ///////////////////////
            //СТРУКТУРА НЕЙРОСЕТИ//
            ///////////////////////
            int NNscructNodeId = h.add("name:NN_sctruct");
            h.addByParentId(NNscructNodeId, "name:layer1,value:Dense,neurons_count:"+ window_size.ToString());
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
    }
}
