﻿using System;
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
        public MainForm form1;
        public Task searchForAgentsTask;
        public Task listener;
        public string status;
        public string IP = "192.168.1.5";
        private const int port = 8888;
        public TcpListener TCPListener;
        public AgentManager(MainForm form1)
        {
            this.form1 = form1;

            searchForAgentsTask = Task.Factory.StartNew(() => { searchForAgents(); });
            tasks = new List<AgentTask>();
            agents = new List<Agent>();

        }

        public void doWork()
        {
            Task working = Task.Factory.StartNew(() => { work(); });
        }

        public void work()
        {
            bool isWorkDone = false;
            bool anyTasks = false;

            status = "working";
            while (!isWorkDone)
            {
                anyTasks = false;
                foreach (AgentTask task in tasks)
                {
                    if (task.status == "undone" | task.status == "working")
                        anyTasks = true;
                }
                if (anyTasks)
                    foreach (AgentTask task in tasks)
                    {
                        if (task.status == "undone")
                        {
                            foreach (Agent agent in agents)
                            {
                                if (agent.status == "free")
                                {
                                   // log(agent.hostName + " - " + task.h.getValueByName("code"));
                                    agent.task = task;
                                    agent.status = "busy";
                                    task.status = "working";
                                    break;
                                }
                            }
                        }
                    }
                else
                    isWorkDone = true;
                System.Threading.Thread.Sleep(100);
            }
            status = "done";
        }

        public void searchForAgents()
        {
            try
            {
                log("IP: " + IP);
                TCPListener = new TcpListener(IPAddress.Parse(IP), port);
                TCPListener.Start();
                log("Менеджер агентов запущен");
                // log("Ожидание подключений...");

                while (true)
                {
                    TcpClient client = TCPListener.AcceptTcpClient();
                    Agent clientObject = new Agent(client, form1);
                    agents.Add(clientObject);
                    agents[agents.Count - 1].ip = client.Client.RemoteEndPoint.ToString().Split(':')[0];
                    agents[agents.Count - 1].hostName = Dns.GetHostEntry(IPAddress.Parse(agents[agents.Count - 1].ip)).HostName;
                    log("Подключён агент " + agents[agents.Count - 1].ip);
                    log("                " + agents[agents.Count - 1].hostName);
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
            form1.log(s);
        }
    }

    public class Agent
    {

        public string ip;
        public string hostName;
        public string status = "free";
        public AgentTask task = null;
        public string workFolder;
        public TcpClient client;
        private MainForm form1;

        public string input;
        public string output;
        public Agent(TcpClient tcpClient, MainForm form1)
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
                    if (task.status != "done")
                    {
                        if (task.type == "train")
                        {
                            //log(hostName + ": " + task.h.getValueByName("code")+" .1");
                            status = "busy";
                            //объявление агенту типа задачи
                            sendCommand(writer, "train");

                            task.h.Save();

                            sendFile(writer, task.h.getValueByName("json_file_path"));
                            System.Threading.Thread.Sleep(10);
                            sendFile(writer, task.h.getValueByName("train_script_path"));
                            System.Threading.Thread.Sleep(10);
                            sendFile(writer, task.h.getValueByName("input_file"));
                            System.Threading.Thread.Sleep(10);
                            var trainingReport = recieveCommand(reader);
                            // log(trainingReport);
                            if (!trainingReport.Contains("Произошла ошибка"))
                            {
                                recieveFile(reader, task.h.getValueByName("json_file_path"));
                                System.Threading.Thread.Sleep(10);
                                recieveFile(reader, task.h.getValueByName("predictions_file_path"));
                                System.Threading.Thread.Sleep(10);
                                recieveFile(reader, task.h.getValueByName("save_folder") + "weights.h5");
                                System.Threading.Thread.Sleep(10);

                               // log(hostName + ": " + task.h.getValueByName("code") + " .2");


                                Hyperparameters hTemp = new Hyperparameters(File.ReadAllText(task.h.getValueByName("json_file_path"), Encoding.Default), form1);
                                
                                hTemp.setValueByName("json_file_path", task.h.getValueByName("json_file_path"));
                                hTemp.setValueByName("predictions_file_path", task.h.getValueByName("predictions_file_path"));
                                hTemp.setValueByName("save_folder", task.h.getValueByName("save_folder"));
                                hTemp.setValueByName("train_script_path", task.h.getValueByName("train_script_path"));
                                hTemp.setValueByName("get_prediction_script_path", task.h.getValueByName("get_prediction_script_path"));
                                hTemp.setValueByName("input_file", task.h.getValueByName("input_file"));


                                task.h = hTemp.Clone();
                                task.h.Save();

                                log(hostName+": " + task.h.getValueByName("code") + " trained;   accuracy = " + task.h.getValueByName("stdDev"));
                                task.status = "done";
                        
                            }
                            else
                            {

                                task.status = "error";
                            }
                            status = "free";
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
            //  log("SEND: " + Command);
            writer.Write(Command);

        }
        public string recieveCommand(BinaryReader reader)
        {
            var Command = reader.ReadString();
            //  log("RECIEVE: " + Command);
            return Command;
        }
        void sendFile(BinaryWriter writer, string path)
        {
            //   log("SEND: " + path);
            writer.Write(Encoding.Default.GetString(File.ReadAllBytes(path)));
        }
        void recieveFile(BinaryReader reader, string savePath)
        {
            File.Delete(savePath);
            string message = reader.ReadString(); ;
            var file = Encoding.Default.GetBytes(message);
            File.WriteAllBytes(savePath, file);
            // log("RECIEVE: " + savePath);
        }


        public void log(string s)
        {
            form1.log(s);
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
