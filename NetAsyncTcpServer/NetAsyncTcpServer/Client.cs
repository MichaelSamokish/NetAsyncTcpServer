using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncTcpServer
{
    public class Client : IClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private byte[] _receiveBuffer;

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
            if(State == ClientState.Connected)
                _stream.Write(data, 0, data.Length);
        }

        public void Connect()
        {
            _client.Connect(_address, _port);
            if (_client.Connected)
            {
                _state = ClientState.Connected;
                _stream = _client.GetStream();
                if(OnConnect != null)
                    OnConnect(this, new EventArgs());
                WaitForRequest();
            }
        }

        public void Disconnect()
        {
            _client.Client.Shutdown(SocketShutdown.Both);
            _stream.Close();
            _client.Close();
            _client = null;
            _state = ClientState.Disconnected;
            if(OnDisconnect != null)
                OnDisconnect(this, new EventArgs());
        }

        private void WaitForRequest()
        {
            _stream = _client.GetStream();
            _stream.BeginRead(_receiveBuffer, 0, _receiveBuffer.Length, Read, null);
        }

        private void Read(IAsyncResult result)
        {
            int size = 0;
            try
            {
                size = _stream.EndRead(result);
            }
            catch (Exception ex)
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
