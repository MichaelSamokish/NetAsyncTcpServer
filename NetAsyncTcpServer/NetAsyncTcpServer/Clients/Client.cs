using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetAsyncTcpServer
{
    public class Client : IClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private byte[] _receiveBuffer;
        private CancellationTokenSource _readTaskCancelationToken;

        private IPAddress _address;
        private int _port;
        private ClientState _state;

        public IPAddress Address
        {
            get { return _address; }
        }

        public int Port
        {
            get { return _port; }
        }

        public ClientState State
        {
            get { return _state; }
        }

        public event ClientDataReceivedEventHandler OnDataReceived;
        public event DisconnectEventHandler OnDisconnect;
        public event ConnectEventHandler OnConnect;

        public Client(IPAddress address, int port)
        {
            
            _address = address;
            _port = port;
            IPEndPoint ep = new IPEndPoint(_address, _port);
            _client = new TcpClient();
            _receiveBuffer = new byte[1024];
        }

        public void Send(byte[] data)
        {
            if (State == ClientState.Connected)
            {
                int size = data.Length;
                var sizeBuffer = BitConverter.GetBytes(size);
                _stream.Write(sizeBuffer, 0, sizeBuffer.Length);
                _stream.Write(data, 0, data.Length);
            }
        }

        public void Connect()
        {
            _client.Connect(_address, _port);
            if (_client.Connected)
            {
                _state = ClientState.Connected;
                _stream = _client.GetStream();
                _readTaskCancelationToken = new CancellationTokenSource();
                Task.Run(() => ReadTaskCallback(_readTaskCancelationToken.Token), _readTaskCancelationToken.Token);
                if(OnConnect != null)
                    OnConnect(this, new EventArgs());
            }
        }

        public void Disconnect()
        {
            if (_state == ClientState.Disconnected)
                return;
            _stream.Close();
            _client.Close();
            _state = ClientState.Disconnected;
            _readTaskCancelationToken.Cancel();
            if(OnDisconnect != null)
                OnDisconnect(this, new EventArgs());
        }

        private void ReadTaskCallback(CancellationToken cancellationToken)
        {
            while (true)
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
                catch (Exception)
                {
                    Disconnect();
                    break;
                }
                cancellationToken.ThrowIfCancellationRequested();
                if (_stream.DataAvailable)
                {
                    byte[] sizeBuffer = new byte[sizeof(int)];
                    _stream.Read(sizeBuffer, 0, sizeBuffer.Length);
                    int size = BitConverter.ToInt32(sizeBuffer, 0);
                    byte[] buffer = new byte[size];
                    _stream.Read(buffer, 0, buffer.Length);
                    if (OnDataReceived != null)
                    {
                        OnDataReceived(this, new DataRecivedEventArgs(size, buffer));
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

        public void Dispose()
        {
            OnConnect = null;
            OnDataReceived = null;
            OnDisconnect = null;
            _stream.Close();
            _client.Close();
            _client = null;
        }
    }
}
