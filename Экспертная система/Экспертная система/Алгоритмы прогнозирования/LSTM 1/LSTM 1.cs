namespace Экспертная_система
{
    public class LSTM_1 : Algorithm
    {
        //public Form1 form1;
       public LSTM_1(Form1 form1, string name, int windowSize) : base(form1, name, windowSize)
        {
            h.add("predicted_column_index:100");
            h.add("window_size:20");
            h.add("features:189");
            h.add("drop_column:<0>");
            h.add("number_of_epochs:10");
            h.add("split_point:0.9");
            h.add("trainScriptPath:" + pathPrefix + "Экспертная система\\Экспертная система\\Алгоритмы прогнозирования\\" + name + "\\trainScript.py");
        }
        public string get_prediction()
        {
            return "ошибка: метод не реализован";
        }

    }
}
