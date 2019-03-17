using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Экспертная_система
{
    class Agent
    {

        TcpClient client = null;
        public string pathPrefix;
        public Algorithm algorithm;
        Form1 form1;
        string address = "192.168.1.7";
        public string workFolder;
        const int port = 8888;

        public Agent(Form1 form1, string address, string workFolder)
        {
            this.form1 = form1;
            this.address = address;
            this.workFolder = workFolder;
            pathPrefix = form1.pathPrefix;

        }

        public void startSocket()
        {

            /* try
             {   */
            client = new TcpClient(address, port);
            NetworkStream stream = client.GetStream();
            BinaryReader reader = new BinaryReader(stream);
            BinaryWriter writer = new BinaryWriter(stream);
            while (true)
            {
                System.Threading.Thread.Sleep(1000);

                var command = recieveCommand(reader);

                if (command == "train")
                {
                    recieveFile(reader, workFolder + "json.txt");
                    System.Threading.Thread.Sleep(100);
                    recieveFile(reader, workFolder + "train_script.py");
                    System.Threading.Thread.Sleep(100);
                    recieveFile(reader, workFolder + "input_file.txt");
                    System.Threading.Thread.Sleep(100);

                    algorithm = new DefaultAlgorithmImpl(form1, "Default");
                    algorithm.Open(new Hyperparameters(File.ReadAllText(workFolder + "json.txt"), form1));
                    algorithm.mainFolder = workFolder;
                    algorithm.h.setValueByName("json_file_path", workFolder + "json.txt");
                    algorithm.h.setValueByName("predictions_file_path", workFolder + "predictions.txt");
                    algorithm.h.setValueByName("save_folder", workFolder);
                    algorithm.h.setValueByName("train_script_path", workFolder + "train_script.py");
                    algorithm.h.setValueByName("input_file", workFolder + "input_file.txt");
                    algorithm.h.setValueByName("path_prefix", pathPrefix);

                    log("START TRAINING");
                    algorithm.train().Wait();

                    if (algorithm.trainingReport.LastIndexOf("СКРИПТ ОБУЧЕНИЯ ") != -1)
                    {
                        sendCommand(writer, algorithm.trainingReport);
                        File.WriteAllText(algorithm.h.getValueByName("json_file_path"), algorithm.h.toJSON(0), Encoding.Default);
                        sendFile(writer, algorithm.h.getValueByName("json_file_path"));
                        System.Threading.Thread.Sleep(100);
                        sendFile(writer, algorithm.h.getValueByName("predictions_file_path"));
                        System.Threading.Thread.Sleep(100);
                        sendFile(writer, algorithm.h.getValueByName("save_folder") + "weights.h5");
                        System.Threading.Thread.Sleep(100);
                    }
                    else
                        sendCommand(writer, "Произошла ошибка при запуске скрипта обучения, подробности в консоли агента.");
                }
            }
            /*  }
              catch (Exception ex)
              {
                  Console.WriteLine(ex.Message);
              }
              finally
              {
                  client.Close();
              }  */
        }
        public void sendCommand(BinaryWriter writer, string Command)
        {
            log("SEND: " + Command);
            writer.Write(Command);

        }
        public string recieveCommand(BinaryReader reader)
        {
            var Command = reader.ReadString();
            log("RECIEVE: " + Command);
            return Command;
        }
        void sendFile(BinaryWriter writer, string path)
        {
            log("SEND: " + path);
            writer.Write(Encoding.Default.GetString(File.ReadAllBytes(path)));
        }
        void recieveFile(BinaryReader reader, string savePath)
        {
            string message = reader.ReadString(); ;
            var file = Encoding.Default.GetBytes(message);
            File.WriteAllBytes(savePath, file);
            log("RECIEVE: " + savePath);
        }
        void log(String s)
        {
            form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
            form1.logBox.Invoke(form1.logDelegate, form1.logBox, s, Color.White);
        }

    }
}
