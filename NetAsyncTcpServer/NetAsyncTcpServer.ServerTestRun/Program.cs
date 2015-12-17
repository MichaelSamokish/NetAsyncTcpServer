using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncTcpServer.ServerTestRun
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpServer server = new TcpServer(IPAddress.Loopback, 65100);
            server.OnOpen += server_OnOpen;
            server.OnClose += server_OnClose;
            server.OnClientConnect += server_OnClientConnect;
            server.OnClientDisconnect += server_OnClientDisconnect;
            server.OnDataRecived += server_OnDataRecived;
            server.Start();
            Console.ReadKey();
            server.Stop();
            Console.ReadKey();
        }

        static void server_OnDataRecived(IConnectionClient sender, DataRecivedEventArgs e)
        {
            Console.WriteLine("Получены данные размером {0} байт\n Содержимое: {1}", e.Size,e.Data[0]);
        }

        static void server_OnClientDisconnect(IConnectionClient client, EventArgs e)
        {
            Console.WriteLine("Отключился клиент");
        }

        static void server_OnClientConnect(IConnectionClient client, EventArgs e)
        {
            Console.WriteLine("Подключился клиент");
            client.Send(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        static void server_OnClose(IServer sender, EventArgs e)
        {
            Console.WriteLine("Сервер остановлен");
        }

        static void server_OnOpen(IServer sender, EventArgs e)
        {
            Console.WriteLine("Сервер запущен");
        }
    }
}
