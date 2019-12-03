using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Экспертная_система
{
    class AgentLink
    {

        TcpClient client = null;
        public string pathPrefix;
        public Algorithm algorithm;
        MainForm form1;
        string address = "";
        public string workFolder;
        const int port = 8888;

        public AgentLink(MainForm form1, string address, string workFolder)
        {
            this.form1 = form1;
            this.address = address;
            this.workFolder = workFolder;
            pathPrefix = form1.pathPrefix;

        }

        public void startSocket()
        {
        tryToConnect:
            try
            {
                client = new TcpClient(address, port);
            }
            catch
            {
                System.Threading.Thread.Sleep(5000);
                goto tryToConnect;
            }
            NetworkStream stream = client.GetStream();
            BinaryReader reader = new BinaryReader(stream);
            BinaryWriter writer = new BinaryWriter(stream);

            log("ПОДКЛЮЧЕНО " + DateTime.Now.TimeOfDay.ToString(), Color.Yellow);

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                try
                {
                    var command = recieveCommand(reader);
                    if (command == " " | command == "error" | command == "")
                    {
                        //ПЕРЕПОДКЛЮЧЕНИЕ
                        client.Close();
                        System.Threading.Thread.Sleep(100);
                        log("СОЕДИНЕНИЕ ЗАКРЫТО " + DateTime.Now.TimeOfDay.ToString(), Color.Yellow);
                        startSocket();
                    }
                    if (command == "train")
                    {
                        recieveFile(reader, workFolder + "h.json");
                        System.Threading.Thread.Sleep(10);
                        recieveFile(reader, workFolder + "train_script.py");
                        System.Threading.Thread.Sleep(10);
                        recieveFile(reader, workFolder + "input_file.txt");
                        System.Threading.Thread.Sleep(10);

                        algorithm = new DefaultAlgorithmImpl(form1, "Default");
                        algorithm.Open(workFolder + "h.json");
                        algorithm.mainFolder = workFolder;
                        algorithm.h.setValueByName("json_file_path", workFolder + "h.json");
                        algorithm.h.setValueByName("predictions_file_path", workFolder + "predictions.txt");
                        algorithm.h.setValueByName("save_folder", workFolder);
                        algorithm.h.setValueByName("train_script_path", workFolder + "train_script.py");
                        algorithm.h.setValueByName("input_file", workFolder + "input_file.txt");
                        algorithm.h.setValueByName("path_prefix", pathPrefix);

                        log("START TRAINING");
                        algorithm.train();

                        if (algorithm.trainingResponse.Contains("успешно завершён"))
                        {
                            sendCommand(writer, algorithm.trainingResponse);
                            File.WriteAllText(algorithm.h.getValueByName("json_file_path"), algorithm.h.toJSON(0), Encoding.Default);
                            sendFile(writer, algorithm.h.getValueByName("json_file_path"));
                            System.Threading.Thread.Sleep(10);
                            sendFile(writer, algorithm.h.getValueByName("predictions_file_path"));
                            System.Threading.Thread.Sleep(10);
                            sendFile(writer, algorithm.h.getValueByName("save_folder") + "weights.h5");
                            System.Threading.Thread.Sleep(10);
                        }
                        else
                            sendCommand(writer, "Произошла ошибка при запуске скрипта обучения, подробности в консоли агента.");
                    }
                }
                catch (Exception ex)
                {
                    log(ex.Message);
                    if (ex.Message.Contains("принудительно"))
                    {
                        client.Close();
                        System.Threading.Thread.Sleep(100);
                        log("СОЕДИНЕНИЕ ЗАКРЫТО " + DateTime.Now.TimeOfDay.ToString(), Color.Yellow);
                        startSocket();
                    }
                }
            }

        }
        public void sendCommand(BinaryWriter writer, string Command)
        {
            log("SEND: " + Command);
            writer.Write(Command);

        }
        public string recieveCommand(BinaryReader reader)
        {
            var Command = "";
            try
            {
                Command = reader.ReadString();
                log("RECIEVE: " + Command);
            }
            catch (Exception ex)
            {
                log(ex.Message);
                return "error";
            }
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
        private void log(String s, Color col)
        {
            form1.log(s, col);
        }
        public void log(string s)
        {
            form1.log(s);
        }
    }
    class DefaultAlgorithmImpl : Algorithm
    {

        public DefaultAlgorithmImpl(MainForm form1, string name) : base(form1, name)
        {
            this.name = name;
        }
        public override void Open(string jsonPath)
        {
            this.h = new Hyperparameters(File.ReadAllText(jsonPath, System.Text.Encoding.Default), form1);
            modelName = getValueByName("model_name");
        }
        public override void Save()
        {
            File.WriteAllText(h.getValueByName("json_file_path"), h.toJSON(0), System.Text.Encoding.Default);
        }
    }
}
