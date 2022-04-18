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
        public int SubPort { get; }
        public int PubPort { get; }
        public IPInfo(IPAddress host, int subport, int pubport)
        {
            Host = host;
            PubPort = pubport;
            SubPort = subport;
        }
    }
}
