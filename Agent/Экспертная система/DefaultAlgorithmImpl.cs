using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace Экспертная_система
{
    class DefaultAlgorithmImpl : Algorithm
    {

        public DefaultAlgorithmImpl(Form1 form1, string name) : base(form1, name)
        {
            this.name = name;
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
