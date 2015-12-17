using NetAsyncTcpServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NeyAsyncTspServer.ClientTestRun
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client(IPAddress.Loopback, 65100);
            client.OnConnect += client_OnConnect;
            client.OnDisconnect += client_OnDisconnect;
            client.Connect();
            for (int i = 0; i < 100; i++)
                client.Send(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 11, 11, 11, 11, 11, 11 });
            Console.ReadKey();
            client.Disconnect();
            Console.ReadKey();
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
