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
                // TCPListener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                log(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString());
                TCPListener = new TcpListener(Dns.GetHostByName(Dns.GetHostName()).AddressList[0], port);
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
            BinaryReader reader = new BinaryReader(stream);
            BinaryWriter writer = new BinaryWriter(stream);
            while (true)
            {
                if (task != null)
                {
                    if (task.status == "undone")
                    {
                        if (task.type == "train")
                        {
                            status = "busy";
                            //объявление агенту типа задачи
                            sendCommand(writer, "train");

                            sendFile(writer, task.h.getValueByName("json_file_path"));
                            System.Threading.Thread.Sleep(1000);
                            sendFile(writer, task.h.getValueByName("train_script_path"));
                            System.Threading.Thread.Sleep(1000);
                            sendFile(writer, task.h.getValueByName("input_file"));
                            System.Threading.Thread.Sleep(1000);
                            var trainingReport = recieveCommand(reader);
                            // log(trainingReport);
                            if (!trainingReport.Contains("Произошла ошибка"))
                            {
                                recieveFile(reader, task.h.getValueByName("json_file_path"));
                                System.Threading.Thread.Sleep(1000);
                                recieveFile(reader, task.h.getValueByName("predictions_file_path"));
                                System.Threading.Thread.Sleep(1000);
                                recieveFile(reader, task.h.getValueByName("save_folder") + "weights.h5");
                                System.Threading.Thread.Sleep(1000);
                                Hyperparameters hTemp = new Hyperparameters(File.ReadAllText(task.h.getValueByName("json_file_path"), Encoding.Default), form1);
                                hTemp.setValueByName("json_file_path", task.h.getValueByName("json_file_path"));
                                hTemp.setValueByName("predictions_file_path", task.h.getValueByName("predictions_file_path"));
                                hTemp.setValueByName("save_folder", task.h.getValueByName("save_folder"));
                                hTemp.setValueByName("train_script_path", task.h.getValueByName("train_script_path"));
                                hTemp.setValueByName("input_file", task.h.getValueByName("input_file"));
                                File.WriteAllText(hTemp.getValueByName("json_file_path"), hTemp.toJSON(0), Encoding.Default);
                                task.h.fromJSON(hTemp.toJSON(0), 0);

                                task.status = "done";
                            }
                            else
                            {
                                status = "free";
                                task.status = "error";
                            }
                        }
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
            log("SEND: "+ path);
            writer.Write(Encoding.Default.GetString(File.ReadAllBytes(path)));
        }
        void recieveFile(BinaryReader reader, string savePath)
        {
            string message = reader.ReadString(); ;
            var file = Encoding.Default.GetBytes(message);
            File.WriteAllBytes(savePath, file);
            log("RECIEVE: " + savePath);
        }
        private void send(NetworkStream stream, string message)
        {
            var data = Encoding.Default.GetBytes(message);
            stream.Write(data, 0, data.Length);
            //  log("send: " + message);
        }
        private byte[] recieveBytes(NetworkStream stream)
        {
            byte[] bytes;
            waitForBytes:
            if (stream.DataAvailable)
            {
                byte[] data = new byte[64];
                StringBuilder builder = new StringBuilder();
                int bytesCount = 0;
                do
                {
                    bytesCount = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Default.GetString(data, 0, bytesCount));
                    System.Threading.Thread.Sleep(50);
                }
                while (stream.DataAvailable);

                string message = builder.ToString();
                bytes = Encoding.Default.GetBytes(message);
                //     log("recieve: bytes");          
            }
            else
            {
                goto waitForBytes;
            }
            return bytes;
        }
        private string recieve(NetworkStream stream)
        {
            string message;

            waitForMessage:
            if (stream.DataAvailable)
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

                message = builder.ToString();
                //log("RECIEVE: " + message);
                //   log("recieve: " + message);  
            }
            else
            {
                goto waitForMessage;
            }
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
