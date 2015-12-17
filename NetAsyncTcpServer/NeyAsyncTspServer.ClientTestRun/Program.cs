using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncTcpServer.ClientTestRun
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client(IPAddress.Loopback, 65100);
            client.OnConnect += client_OnConnect;
            client.OnDisconnect += client_OnDisconnect;
            client.OnDataReceived += client_OnDataReceived;
            client.Connect();
            for (int i = 0; i < 10000; i++)
                client.Send(new byte[] { 1, 2, 3, 4 });
            Console.ReadKey();
            client.Disconnect();
            Console.ReadKey();
        }

        static void client_OnDataReceived(IClient client, DataRecivedEventArgs e)
        {
            Console.WriteLine("данные от сервера {0} байт", e.Size);
        }

        static void client_OnDisconnect(IClient sender, EventArgs e)
        {
            Console.WriteLine("Отключен от сервера");
        }

        static void client_OnConnect(IClient sender, EventArgs e)
        {
            Console.WriteLine("Подключен к серверу");
        }
    }
}
