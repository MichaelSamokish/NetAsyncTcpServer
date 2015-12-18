using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncTcpServer
{
    public enum ClientState
    {
        Disconnected = 0,
        Connected = 1
    }

    public delegate void ClientDataReceivedEventHandler(IClient client, DataRecivedEventArgs e);
    public delegate void ConnectEventHandler(IClient sender, EventArgs e);
    public delegate void DisconnectEventHandler(IClient sender, EventArgs e);
    public interface IClient : IDisposable
    {
        IPAddress Address { get; }
        int Port { get; }
        ClientState State { get; }

        event ClientDataReceivedEventHandler OnDataReceived;
        event DisconnectEventHandler OnDisconnect;
        event ConnectEventHandler OnConnect;

        void Send(byte[] data);
        void Connect();
        void Disconnect();
    }
}
