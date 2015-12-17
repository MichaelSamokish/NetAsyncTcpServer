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

        private CancellationTokenSource _readTaskCancelationToken;

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
            _readTaskCancelationToken = new CancellationTokenSource();
            Task.Run(() => ReadTaskCallback(_readTaskCancelationToken.Token), _readTaskCancelationToken.Token);
        }

        public void Send(byte[] data)
        {
            var stream = _client.GetStream();
            byte[] sizeBuffer = BitConverter.GetBytes(data.Length);
            stream.Write(sizeBuffer, 0, sizeBuffer.Length);
            stream.Write(data, 0, data.Length);
        }

        public void Disconnect()
        {
            _stream.Close();
            _client.Close();
            _readTaskCancelationToken.Cancel();
            if(OnDisconnect != null)
                OnDisconnect(this, new EventArgs());
        }

        public void Dispose()
        {
            _client.Close();
            _client.Client.Dispose();
        }

        private void ReadTaskCallback(CancellationToken cancellationToken)
        {
            while(true)
            {
                if (!IsConnected())
                {
                    Disconnect();
                    break;
                }
                try
                {
                    _stream = _client.GetStream();
                }
                catch(Exception)
                {
                    Disconnect();
                    break;
                }
                cancellationToken.ThrowIfCancellationRequested();
                if(_stream.DataAvailable)
                {
                    byte[] sizeBuffer = new byte[sizeof(int)];
                    _stream.Read(sizeBuffer, 0, sizeBuffer.Length);
                    int size = BitConverter.ToInt32(sizeBuffer, 0);
                    byte[] buffer = new byte[size];
                    _stream.Read(buffer, 0, buffer.Length);
                    if(OnDataReceived != null)
                    {
                        OnDataReceived(this,new DataRecivedEventArgs(size,buffer));
                    }
                }

            }
        }

        private bool IsConnected()
        {
            Socket soc = _client.Client;
            try
            {
                return !(soc.Poll(1, SelectMode.SelectRead) && soc.Available == 0);
            }
            catch (SocketException) { return false; }
        }

    }
}
