using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PubSubLib
{
    public class IPInfo
    {
        public IPAddress Host { get; }
        public int Port { get; }
        public IPInfo(IPAddress host, int port)
        {
            Host = host;
            Port = port;
        }
    }
}
