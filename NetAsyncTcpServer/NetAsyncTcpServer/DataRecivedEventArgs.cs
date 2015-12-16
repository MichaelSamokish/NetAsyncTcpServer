using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncTcpServer
{
    public class DataRecivedEventArgs
    {
        public byte[] Data { get; private set; }
        public int Size { get; private set; }
        public DataRecivedEventArgs(int size, byte[] data)
        {
            Data = data;
            Size = size;
            TcpServer s = new TcpServer(System.Net.IPAddress.Any, 80);
        }

    }
}
