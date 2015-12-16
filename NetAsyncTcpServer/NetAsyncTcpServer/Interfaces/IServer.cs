using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncTcpServer
{
    public enum ServerState
    {
        Closed = 0,
        Opened = 1
    }

    public delegate void ServerEventHandler(IServer sender, EventArgs e);
    public delegate void ClientConnectedEventHandler(IConnectionClient client, EventArgs e);
    public delegate void ClientDisonnectedEventHandler(IConnectionClient client, EventArgs e);

    public interface IServer
    {
        ServerState State { get; }
        int ListenedPort { get; }
        IEnumerable<IConnectionClient> Clients { get; }
        
        event ServerEventHandler OnOpen;
        event ServerEventHandler OnClose;
        event ClientConnectedEventHandler OnClientConnect;
        event ClientDisonnectedEventHandler OnClientDisconnect;
        event DataReceivedEventHandler OnDataRecived;

        void Start();
        void Stop();
        void DisconnectClient(IConnectionClient client);
        void DisconnectClient(Guid clientUid);
        void Send(IConnectionClient client, byte[] data);
        void Send(Guid clientUid, byte[] data);
        void SendToAll(byte[] data);
        void SendToAllExludeOne(IConnectionClient client, byte[] data);
        void SendToAllExludeOne(Guid clientUid, byte[] data);
    }
}
