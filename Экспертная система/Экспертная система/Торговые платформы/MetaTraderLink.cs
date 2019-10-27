using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
namespace Экспертная_система
{
    class MetaTraderLink
    {
        private MainForm mainForm;
        public Socket handler;

        public string ACTION = "";
        //string IP = "192.168.1.2";

     //string IP = "localhost";
        public MetaTraderLink(MainForm mainForm)
        {
            this.mainForm = mainForm;


            // получаем адреса для запуска сокета
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Loopback, Convert.ToInt32("80"));

            // создаем сокет
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // связываем сокет с локальной точкой, по которой будем принимать данные
            listenSocket.Bind(ipPoint);

            // начинаем прослушивание
            listenSocket.Listen(10);

            Task listen = new Task(() =>
            {
                mainForm.log("Ожидание подключения MetaTrader.. ");
                while (true)
                {
                    try
                    {
                        handler = listenSocket.Accept();
                        mainForm.log("MetaTrader подключён.. ");
                        while (true)
                        {

                            // получаем сообщение
                            StringBuilder builder = new StringBuilder();
                            int bytes = 0; // количество полученных байтов
                            byte[] data = new byte[256]; // буфер для получаемых данных

                            do
                            {
                                bytes = handler.Receive(data);
                                builder.Append(Encoding.Default.GetString(data, 0, bytes));
                            }
                            while (handler.Available > 0);

                            string request = builder.ToString();
                            string response = getResponse(request);
                            send(response);
                            System.Threading.Thread.Sleep(1);
                        }

                    }
                    catch (Exception e)
                    {
                        mainForm.log(e.Message);
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                        mainForm.log("Подключение разорвано");
                    }
                }
            });
            listen.Start();
        }
        public DateTime actualTime;
        public string getResponse(string request)
        {
            DateTime dt;
            if (DateTime.TryParse(request, out dt))
            {
                actualTime = dt;
            }
            else
            if (request == "get_action")
            {
                string res = ACTION;
                ACTION = "";
                return res;
            }
            else
            if (request == "shutdown")
            {
                mainForm.Form1_FormClosing(null,null);
            }
            return "";
        }
        public void send(string s)
        {
            byte[] data = System.Text.Encoding.Default.GetBytes(s);
            handler.Send(data);

        }
        public string recv()
        {
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            byte[] data = new byte[256];

            do
            {
                bytes = handler.Receive(data);
                builder.Append(Encoding.Default.GetString(data, 0, bytes));
            }
            while (handler.Available > 0);

            string s = builder.ToString();
            return s;
        }
        public double getBuyPrice_Ask(string symbol)
        {
            //отправить запрос аск
            throw new NotImplementedException();
        }

        public double getSellPrice_Bid(string symbol)
        {
            //отправить запрос бид
            throw new NotImplementedException();
        }

        public double getResponseDouble(string request)
        {
            throw new NotImplementedException();
        }
    }
}
