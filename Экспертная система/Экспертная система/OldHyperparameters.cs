using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Экспертная_система
{
    [Serializable]
    public class OldHyperparameters
    {
        public OldHyperparameters(Form1 form1)
        {
            this.form1 = form1;
        }

        [NonSerializedAttribute]
        Form1 form1;

        public List<Parameter> nodes = new List<Parameter>();



        public void add(string name, string category, string categories)
        {
            Parameter parameter = new Parameter();
            parameter.name = name;
            parameter.category = category;
            parameter.categories = categories.Split(',');
            parameter.is_categorical = true;
            parameter.is_change = false;
            parameter.is_change_up_or_down = false;
            parameter.is_const = false;
            nodes.Add(parameter);

        }
        //CONST
        public void add(string name, string value)
        {
            Parameter parameter = new Parameter();
            parameter.name = name;
            parameter.category = value;
            parameter.is_categorical = true;
            parameter.is_const = true;
            nodes.Add(parameter);

        }
        //CONST
        public void add(string name, int value)
        {
            Parameter parameter = new Parameter();
            parameter.name = name;
            parameter.value = value;
            parameter.is_categorical = false;
            parameter.is_const = true;
            nodes.Add(parameter);

        }
        public void add(string name, int value, int min, int max)
        {
            Parameter parameter = new Parameter();
            parameter.name = name;
            parameter.value = value;
            parameter.min = min;
            parameter.max = max;
            parameter.is_categorical = false;
            parameter.is_const = false;
            nodes.Add(parameter);

        }
        public string get_value_by_name(string name)
        {
            Parameter parameter = null;
            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i].name == name)
                    parameter = nodes[i];
            if (parameter == null)
            {
                log("параметр " + name + " не найден", System.Drawing.Color.Red);
                return "параметр " + name + " не найден";
            }
            if (parameter.is_categorical)
            {
                return parameter.category;
            }
            else
            {
                return parameter.value.ToString();
            }
        }
        public Parameter get_parameter_by_name(string name)
        {
            Parameter parameter = null;
            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i].name == name)
                    parameter = nodes[i];
            if (parameter == null)
            {
                log("параметр " + name + " не найден", System.Drawing.Color.Red);
            }
            return parameter;
        }
        public void set_parameter_by_name(string name, int value)
        {
            bool is_success = false;
            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i].name == name)
                {
                    if (nodes[i].is_categorical)
                    {
                        nodes[i].category = value.ToString();
                        is_success = true;
                    }
                    else
                    {
                        nodes[i].value = value; is_success = true;
                    }
                }
            if (!is_success)
            {
                log("параметр " + name + " не найден", System.Drawing.Color.Red);
            }
        }
        public void set_parameter_by_name(string name, string value)
        {
            bool is_success = false;

            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i].name == name)
                {
                    if (nodes[i].is_categorical)
                    {
                        nodes[i].category = value;
                        is_success = true;
                    }
                    else
                    {
                        nodes[i].value = Convert.ToInt32(value);
                        is_success = true;
                    }
                }

            if (!is_success)
            {
                log("параметр " + name + " не найден", System.Drawing.Color.Red);
            }

        }

        public void variate(string name)
        {
            Parameter param = get_parameter_by_name(name);
            Random r = new Random();
            if (!param.is_const)
                if (param.is_categorical)
                {
                    if (param.categories != null)
                    {
                        string lastValue = param.category;
                        param.category = param.categories[r.Next(param.categories.Length)];
                        if (lastValue == param.category)
                        {
                            param.is_change = true;
                        }
                        else
                        {
                            param.is_change = false;
                        }
                    }
                    else
                    {
                        log("не удалось варьировать категориальный параметр: множество категорий пустое", System.Drawing.Color.Red);
                    }
                }
                else
                {
                    int last_value = 0;
                    int new_value = 0;

                    new_value = r.Next(param.min, param.max);

                    if (new_value > param.value)
                        param.is_change_up_or_down = true;

                    if (new_value < param.value)
                        param.is_change_up_or_down = false;

                    if (last_value != new_value)
                    {
                        param.value = new_value;
                        param.is_change = true;
                    }
                    else
                    {
                        param.is_change = false;
                    }
                }
        }

        override public string ToString()
        {
            string s = "";
            foreach (Parameter param in nodes)
            {
                s = s + "     " + param.name + " = ";
                if (param.is_categorical)
                    s = s + param.category + '\n';
                else
                    s = s + param.value.ToString() + '\n';
            }
            return s;
        }
        void log(String s, System.Drawing.Color col)
        {
            form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
            form1.logBox.Invoke(form1.logDelegate, form1.logBox, new System.Diagnostics.StackTrace().ToString() + s, col);
        }
    }

    //⬤
    [Serializable]
    public class Parameter
    {
        public List<Parameter> nodes;
        public string name;
        public int value;
        public string category;
        public bool is_categorical;
        public int min;
        public int max;
        public string[] categories;
        public bool is_change;
        public bool is_change_up_or_down;
        public bool is_const;
    }
}
