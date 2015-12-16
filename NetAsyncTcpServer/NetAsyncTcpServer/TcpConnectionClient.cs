using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetAsyncTcpServer
{
    public class TcpConnectionClient : IConnectionClient
    {
        private EndPoint _endPoint;
        private IServer _parentServer;
        private Guid _uid;
        private byte[] _receiveBuffer;
        private NetworkStream _stream;

        private TcpClient _client;

        public EndPoint EndPoint
        {
            get { return _endPoint; }
        }
        public IServer ParentServer
        {
            get { return _parentServer; }
        }

        public string Name
        {
            get;
            set;
        }

        public Guid Uid
        {
            get { return _uid; }
        }

        public event DataReceivedEventHandler OnDataReceived;
        public event ClientDisconnectEventHandler OnDisconnect;

        public TcpConnectionClient(TcpClient client,IServer parentServer)
            : this(client,parentServer,1024)
        {

        }
        public TcpConnectionClient(TcpClient client, IServer parentServer, int bufferSize)
        {
            _client = client;
            _uid = Guid.NewGuid();
            _parentServer = parentServer;
            _endPoint = _client.Client.LocalEndPoint;
            _receiveBuffer = new byte[bufferSize];
            WaitForRequest();
            
            //_listeningReadThread = new Thread(Read);
            //_listeningReadThread.Start();
            
        }

        public void Send(byte[] data)
        {
            var stream = _client.GetStream();
            stream.Write(data, 0, data.Length);
        }

        public void Disconnect()
        {
            _stream.Close();
            _client.Close();
            if(OnDisconnect != null)
                OnDisconnect(this, new EventArgs());
        }

        public void Dispose()
        {
            _client.Close();
            _client.Client.Dispose();
        }

        private void WaitForRequest()
        {
            _stream = _client.GetStream();
            _stream.BeginRead(_receiveBuffer, 0, _receiveBuffer.Length, Read, null);
        }

        private void Read(IAsyncResult asyn)
        {
            int size = 0;
            try
            {
                size = _stream.EndRead(asyn);
            }
            catch(Exception ex)
            {
                Disconnect();
                return;
            }
            if (size == 0)
            {
                Disconnect();
                return;
            }
            if (size > 0)
            {
                List<byte> data = new List<byte>();
                for (int i = 0; i < size; i++)
                {
                    data.Add(_receiveBuffer[i]);
                }
                if (data.Count != 0)
                {
                    if (OnDataReceived != null)
                    {
                        OnDataReceived(this, new DataRecivedEventArgs(data.Count, data.ToArray()));
                    }
                }
            }
            
            WaitForRequest();
        }

        
    }
}
