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
    public class TcpServer : IServer
    {
        private TcpListener _listener;

        private ServerState _state;
        private int _listenedPort;
        private List<IConnectionClient> _clients;
        private bool _canListenFlag = false;

        public ServerState State
        {
            get { return _state; }
        }

        public int ListenedPort
        {
            get { return _listenedPort; }
        }

        public IEnumerable<IConnectionClient> Clients
        {
            get { return _clients; }
        }

        public event ServerEventHandler OnOpen;
        public event ServerEventHandler OnClose;
        public event ClientConnectedEventHandler OnClientConnect;
        public event ClientDisonnectedEventHandler OnClientDisconnect;
        public event DataReceivedEventHandler OnDataRecived;


        public TcpServer(IPAddress ip, int port)
        {
            _listener = new TcpListener(ip, port);
            _listenedPort = port;
            _clients = new List<IConnectionClient>();
        }

        public void Start()
        {
            try
            {
                _listener.Start();
            }
            catch(Exception ex)
            {
                throw ex;
            }

            _canListenFlag = true;
            StartListening();

            _state = ServerState.Opened;
            if(OnOpen != null)
                OnOpen(this,new EventArgs());
        }

        public void Stop()
        {
            _canListenFlag = false;
            _listener.Stop();
            _state = ServerState.Closed;
            if(OnClose != null)
                OnClose(this, new EventArgs());
        }

        public void DisconnectClient(IConnectionClient client)
        {
            client.OnDisconnect -= Client_DisconnectClient;
            client.OnDataReceived -= DataReceived;
            client.Disconnect();
            if (OnClientDisconnect != null)
                OnClientDisconnect(client, new EventArgs());
            _clients.Remove(client);
        }

        private void Client_DisconnectClient(IConnectionClient sender, EventArgs e)
        {
            DisconnectClient(sender);
        }

        public void DisconnectClient(Guid clientUid)
        {
            var client = _clients.Find(c => c.Uid == clientUid);
            if(client != null)
            {
                DisconnectClient(client);
            }
        }

        public void Send(IConnectionClient client, byte[] data)
        {
            client.Send(data);
        }

        public void Send(Guid clientUid, byte[] data)
        {
            var client = _clients.Find(c => c.Uid == clientUid);
            if (client != null)
            {
                client.Send(data);
            }
        }

        public void SendToAll(byte[] data)
        {
            foreach(var client in _clients)
            {
                client.Send(data);
            }
        }

        public void SendToAllExludeOne(IConnectionClient client, byte[] data)
        {
            foreach (var cl in _clients)
            {
                if(cl != client)
                    cl.Send(data);
            }
        }

        public void SendToAllExludeOne(Guid clientUid, byte[] data)
        {
            foreach (var cl in _clients)
            {
                if (cl.Uid != clientUid)
                    cl.Send(data);
            }
        }

        private async void StartListening()
        {
            while (_canListenFlag)
            {
                TcpClient client = null;
                try
                {
                     client = await _listener.AcceptTcpClientAsync();
                }
                catch(ObjectDisposedException ex)
                {
                    _canListenFlag = false;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                if(client != null)
                {
                    var connectionClient = new TcpConnectionClient(client, this);
                    connectionClient.OnDataReceived += DataReceived;
                    connectionClient.OnDisconnect += Client_DisconnectClient;
                    if (OnClientConnect != null)
                        OnClientConnect(connectionClient, new EventArgs());
                    _clients.Add(connectionClient);
                }
            }
        }

        private void DataReceived(IConnectionClient sender, DataRecivedEventArgs e)
        {
            if(OnDataRecived != null)
                OnDataRecived(sender, e);
        }


        
    }
}
