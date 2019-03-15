using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Экспертная_система
{
    public class AgentManager
    {
        public List<Agent> agents;
        public List<AgentTask> tasks;
        public Form1 form1;
        public Task searchForAgentsTask;
        public Task listener;
        public string status;
        private const int port = 8888;
        private static TcpListener TCPListener;
        public AgentManager(Form1 form1)
        {
            this.form1 = form1;

            searchForAgentsTask = Task.Factory.StartNew(() => { searchForAgents(); });
            tasks = new List<AgentTask>();
            agents = new List<Agent>();
        }
        public void doWork()
        {
            searchForAgentsTask = Task.Factory.StartNew(() => { work(); });
        }
        public void work()
        {
            bool isWorkDone = false;
            while (!isWorkDone)
            {
                foreach (AgentTask task in tasks)
                {
                    if (task.status == "undone")
                        isWorkDone = false;
                }
                foreach (AgentTask task in tasks)
                {
                    if (task.status == "undone")
                    {
                        foreach (Agent agent in agents)
                        {
                            if (agent.status == "free")
                            {
                                agent.task = task;
                            }
                        }
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        public void searchForAgents()
        {
            try
            {
                TCPListener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                TCPListener.Start();
                log("Ожидание подключений...");

                while (true)
                {
                    TcpClient client = TCPListener.AcceptTcpClient();
                    Agent clientObject = new Agent(client, form1);
                    agents.Add(clientObject);
                    log("Найден новый агент:" + client.Client.LocalEndPoint.ToString());
                    // создаем новый поток для обслуживания нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                log(ex.Message);
            }
            finally
            {
                if (TCPListener != null)
                    TCPListener.Stop();
            }
        }

        private void log(String s)
        {
            form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
            form1.logBox.Invoke(form1.logDelegate, form1.logBox, s, System.Drawing.Color.White);
        }
    }

    public class Agent
    {
        public string ip;
        public string status = "free";
        public AgentTask task = null;
        public string workFolder;
        public TcpClient client;
        private Form1 form1;

        public string input;
        public string output;
        public Agent(TcpClient tcpClient, Form1 form1)
        {
            client = tcpClient;
            this.form1 = form1;

        }

        public void Process()
        {
            NetworkStream stream = null;
            /* try
             {   */
            stream = client.GetStream();
            while (true)
            {
                if (task != null)
                {
                    if (task.type == "train")
                    {
                        status = "busy";

                        send(stream, "send_files");
                        if (recieve(stream) == "wait for files")
                            client.Client.SendFile(task.h.getValueByName("json_file_path"));
                        if (recieve(stream) == "file recieved")
                            client.Client.SendFile(task.h.getValueByName("train_script_path"));
                        if (recieve(stream) == "file recieved")
                            client.Client.SendFile(task.h.getValueByName("input_file"));
                        if (recieve(stream) == "file recieved")
                            send(stream, "train");

                        var response = recieve(stream);
                        send(stream, "OK");
                        if (response == "success")
                        {
                            log(recieve(stream));
                            send(stream, "OK");
                            var json_file = recieveBytes(stream);
                            File.WriteAllBytes(task.h.getValueByName("json_file_path"), json_file);
                            send(stream, "file recieved");
                            var predictions_file = recieveBytes(stream);
                            File.WriteAllBytes(task.h.getValueByName("predictions_file_path"), predictions_file);
                            send(stream, "file recieved");
                            var h5_file = recieveBytes(stream);
                            File.WriteAllBytes(task.h.getValueByName("save_folder") + "weights.h5", h5_file);
                            send(stream, "file recieved");

                            Hyperparameters hTemp = new Hyperparameters(File.ReadAllText(task.h.getValueByName("json_file_path"), Encoding.Default), form1);
                            hTemp.setValueByName("json_file_path", task.h.getValueByName("json_file_path"));
                            hTemp.setValueByName("predictions_file_path", task.h.getValueByName("predictions_file_path"));
                            hTemp.setValueByName("save_folder", task.h.getValueByName("save_folder"));
                            hTemp.setValueByName("train_script_path", task.h.getValueByName("train_script_path"));
                            hTemp.setValueByName("input_file", task.h.getValueByName("input_file"));
                            File.WriteAllText(hTemp.getValueByName("json_file_path"), hTemp.toJSON(0), Encoding.Default);
                            task.h.fromJSON(hTemp.toJSON(0),0);
                        }
                        else
                        {
                            log(recieve(stream));
                        }
                        //копирование файлов json.txt и predictions.txt
                        //{восстановить пути под исходное окружение}
                        //выставить статус "free"

                        task = null;
                    }

                }
                System.Threading.Thread.Sleep(1000);
            }
            /*   }
               catch (Exception ex)
               {
                   log(ex.Message);
               }
               finally
               {
                   if (stream != null)
                       stream.Close();
                   if (client != null)
                       client.Close();
               } */
        }
        private void send(NetworkStream stream, string message)
        {
            var data = Encoding.Default.GetBytes(message);
            stream.Write(data, 0, data.Length);
            log("SEND: " + message);
        }
        private byte[] recieveBytes(NetworkStream stream)
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytesCount = 0;
            do
            {
                bytesCount = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Default.GetString(data, 0, bytesCount));
            }
            while (stream.DataAvailable);

            string message = builder.ToString();
            byte[] bytes = Encoding.Default.GetBytes(message);
            return bytes;
        }
        private string recieve(NetworkStream stream)
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Default.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);

            string message = builder.ToString();
            log("RECIEVE: " + message);
            return message;


        }

        public void log(String s)
        {
            form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
            form1.logBox.Invoke(form1.logDelegate, form1.logBox, s, System.Drawing.Color.White);
        }
    }

    public class AgentTask
    {
        public string type;
        public string status;
        public Hyperparameters h;
        public AgentTask(string type, Hyperparameters h)
        {
            this.type = type;
            this.h = h;
            status = "undone";
        }

    }
}
