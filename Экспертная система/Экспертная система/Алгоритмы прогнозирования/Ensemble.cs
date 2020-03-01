using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Экспертная_система
{
    public class Ensemble
    {
        MainForm mainForm;

        public List<Algorithm> algorithms;

        public Ensemble(MainForm mainForm)
        {
            this.mainForm = mainForm;
            algorithms = new List<Algorithm>();
        }

        public void cyclicPrediction(bool runCyclePredScript)
        {
            if (runCyclePredScript)
            {
                List<Task> tasks = new List<Task>();
                foreach (Algorithm algorithm in algorithms)
                {
                    tasks.Add(new Task(() =>
                {
                    mainForm.I.executePythonScript(mainForm.pathPrefix + @"\Экспертная система\Экспертная система\Алгоритмы прогнозирования\" + algorithm.name + @"\cyclic_prediction.py", "--json_file_path \"" + algorithm.getValueByName("json_file_path") + '\"');
                }));
                    tasks[tasks.Count - 1].Start();
                }
                foreach (Task task in tasks)
                    task.Wait();
            }
            mainForm.vis.clear();
            mainForm.vis.enableGrid = true;

            mainForm.vis.addParameter("Ensemble", Color.Red, 900);

            mainForm.vis.parameters[0].functions.Add(new Function("_real", Color.Red, 3));
            mainForm.vis.parameters[0].functions.Add(new Function("Ensemble prediction", Color.Cyan, 4));
            mainForm.vis.parameters[0].functions.Add(new Function("Графики должны совпадать", Color.DarkGray));

            int predicted_column_index = Convert.ToInt16(algorithms[0].getValueByName("predicted_column_index"));

            List<double[]> ensemble_predictions = new List<double[]>();
            foreach (Algorithm algorithm in algorithms)
            {
                int steps_forward = int.Parse(algorithm.h.getValueByName("steps_forward"));
                var predictionsCSV = File.ReadAllLines(algorithm.getValueByName("save_folder") + "\\cyclic_prediction.txt");
                var features = predictionsCSV[1].Split(';');
                var n = algorithm.getValueByName("model_name");

                string alg_name = algorithm.getValueByName("model_name");
                mainForm.vis.addParameter(alg_name, ParameterVisualizer.valueToColor(0, algorithms.Count, algorithms.Count - algorithms.IndexOf(algorithm) - 1), 300);
                mainForm.vis.parameters[mainForm.vis.parameters.Count - 1].functions.Add(new Function(alg_name + "real", Color.Red, 1));
                mainForm.vis.parameters[mainForm.vis.parameters.Count - 1].functions.Add(new Function(alg_name + " prediction", Color.Cyan, 1));

                double[] algorithm_predictions = new double[predictionsCSV.Length - steps_forward - 1];
                for (int i = 1; i < predictionsCSV.Length - steps_forward; i++)
                {
                    double predictedValue;
                    double realValue;

                    predictedValue = Convert.ToDouble(predictionsCSV[i].Split(';')[features.Length - 2].Replace('.', ','));
                    realValue = Convert.ToDouble(predictionsCSV[i + steps_forward].Split(';')[predicted_column_index].Replace('.', ','));

                    algorithm_predictions[i - 1] = predictedValue;

                    mainForm.vis.addPoint(realValue, alg_name + "real");
                    mainForm.vis.addPoint(predictedValue, alg_name + " prediction");

                    if (algorithms.IndexOf(algorithm) == 0)
                        mainForm.vis.addPoint(realValue, "_real");
                }
                ensemble_predictions.Add(algorithm_predictions);
            }

            for (int j = 0; j < ensemble_predictions[0].Length; j++)
            {
                double sum = 0;
                int count = 0;
                for (int i = 0; i < ensemble_predictions.Count; i++)
                {
                    if (ensemble_predictions[i].Length > j)
                    {
                        sum += ensemble_predictions[i][j];
                        count++;
                    }
                }
                mainForm.vis.addPoint(sum / count, "Ensemble prediction");
            }
            mainForm.vis.refresh();

        }

        public void stepByStepPrediction()
        {
            bool pred_chart_must_match_original_chart = false;
            mainForm.vis.clear();
            mainForm.vis.enableGrid = true;

            mainForm.vis.addParameter("Ensemble", Color.Red, 900);

            mainForm.vis.parameters[0].functions.Add(new Function("real", Color.Red, 3));
            mainForm.vis.parameters[0].functions.Add(new Function("Ensemble prediction", Color.Cyan, 4));
            if (pred_chart_must_match_original_chart)
                mainForm.vis.parameters[0].functions.Add(new Function("Графики должны совпадать", Color.DarkGray));
            mainForm.vis.parameters[0].showLastNValues = true;
            mainForm.vis.parameters[0].window = 50;

            int predicted_column_index = Convert.ToInt16(algorithms[0].getValueByName("predicted_column_index"));

            List<double[]> ensemble_predictions = new List<double[]>();
            foreach (Algorithm algorithm in algorithms)
            {
                int steps_forward = 0;
                if (pred_chart_must_match_original_chart)
                    steps_forward = int.Parse(algorithm.h.getValueByName("steps_forward"));

                string model_name = algorithm.getValueByName("model_name");
                var predictionsCSV = File.ReadAllLines(algorithm.getValueByName("save_folder") + "\\predictions.txt");
                predictionsCSV = Expert.skipEmptyLines(predictionsCSV);
                var features = predictionsCSV[1].Split(';');
                var n = model_name;
                mainForm.vis.parameters[0].functions.Add(
                    new Function(model_name,
                    ParameterVisualizer.valueToColor(0, algorithms.Count, algorithms.Count - algorithms.IndexOf(algorithm) - 1))
                    );

                double[] algorithm_predictions = new double[predictionsCSV.Length-1];

                for (int i = 1; i < predictionsCSV.Length - steps_forward; i++)
                {
                    double predictedValue;
                    double realValue;

                    predictedValue = Convert.ToDouble(predictionsCSV[i].Split(';')[features.Length - 1].Replace('.', ','));

                    realValue = Convert.ToDouble(predictionsCSV[i + steps_forward].Split(';')[predicted_column_index].Replace('.', ','));


                   /* if (i == 1)
                    {
                        // добавление сдвига к графику прогноза, чтобы он совпадал с реальным значением
                        for (int q = 0; q < steps_forward; q++)
                        {
                            mainForm.vis.addPoint(predictedValue, model_name);
                        }
                    }*/

                    algorithm_predictions[i - 1] = predictedValue;

                    if (algorithms.IndexOf(algorithm) == 0)
                        mainForm.vis.addPoint(realValue, "real");
                    mainForm.vis.addPoint(predictedValue, model_name);
                }
                ensemble_predictions.Add(algorithm_predictions);
            }
          /*  for (int q = 0; q < int.Parse(algorithms[0].h.getValueByName("steps_forward")); q++)
            {////////////////////////////////////////////////////////////
             /////////////// и это лишнее
             // добавление сдвига к графику прогноза, чтобы он совпадал с реальным значением
                mainForm.vis.addPoint(0, "Ensemble prediction")
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            }*/
            for (int j = 0; j < ensemble_predictions[0].Length; j++)
            {
                double sum = 0;
                int count = 0;
                for (int i = 0; i < ensemble_predictions.Count; i++)
                {
                    if (ensemble_predictions[i].Length > j)
                    {
                        sum += ensemble_predictions[i][j];
                        count++;
                    }
                }
                mainForm.vis.addPoint(sum / count, "Ensemble prediction");
            }

            mainForm.vis.refresh();

        }
    }
}
