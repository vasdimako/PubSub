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
        public int SubPort { get; set; }
        public int SubRePort { get; set; }
        public int PubRePort { get; set; }
        public int PubPort { get; }
        public IPInfo(IPAddress host, int subport, int pubport)
        {
            Host = host;
            PubPort = pubport;
            PubRePort = pubport + 5;
            SubPort = subport;
            SubRePort = subport + 5;
        }
    }
}
