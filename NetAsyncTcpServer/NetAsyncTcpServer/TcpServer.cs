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
        private Thread _clientAcceptThread;

        private ServerState _state;
        private int _listenedPort;
        private List<IConnectionClient> _clients;

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
            _clientAcceptThread = new Thread(AcceptClient);
            _clientAcceptThread.Start();

            _state = ServerState.Opened;
            if(OnOpen != null)
                OnOpen(this,new EventArgs());
        }

        public void Stop()
        {
            _listener.Stop();
            _clientAcceptThread.Abort();
            _clientAcceptThread.Join();
            _state = ServerState.Closed;
            if(OnClose != null)
                OnClose(this, new EventArgs());
            
        }

        public void DisconnectClient(IConnectionClient client)
        {
            _clients.Remove(client);
            client.Disconnect();
            
        }

        private void Client_DisconnectClient(IConnectionClient sender, EventArgs e)
        {
            sender.OnDisconnect -= Client_DisconnectClient;
            sender.OnDataReceived -= DataReceived;
            if(OnClientDisconnect != null)
                OnClientDisconnect(sender, e);
            _clients.Remove(sender);
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

        private void AcceptClient()
        {
            while(true)
            {
                var client = _listener.AcceptTcpClient();
                var connectionClient = new TcpConnectionClient(client, this);
                connectionClient.OnDataReceived += DataReceived;
                connectionClient.OnDisconnect += Client_DisconnectClient;
                if(OnClientConnect != null)
                    OnClientConnect(connectionClient, new EventArgs());
                _clients.Add(connectionClient);
            }
        }

        private void DataReceived(IConnectionClient sender, DataRecivedEventArgs e)
        {
            if(OnDataRecived != null)
                OnDataRecived(sender, e);
        }


        
    }
}
