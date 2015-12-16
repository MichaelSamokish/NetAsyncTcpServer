using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncTcpServer
{
    public delegate void DataReceivedEventHandler(IConnectionClient sender, DataRecivedEventArgs e);
    public delegate void ClientDisconnectEventHandler(IConnectionClient sender, EventArgs e);
    public interface IConnectionClient : IDisposable
    {
        EndPoint EndPoint { get; }
        IServer ParentServer { get; }
        String Name { get; set; }
        Guid Uid { get; }

        event DataReceivedEventHandler OnDataReceived;
        event ClientDisconnectEventHandler OnDisconnect;

        void Send(byte[] data);
        void Disconnect();
        void Dispose();

    }
}
