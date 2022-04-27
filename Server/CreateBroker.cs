using PubSubLib;
using System.Net;

IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
// IPAddress ipAddress = ipHostInfo.AddressList[0];
IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

// ip, subport, pubport, msgport - incoming sub, incoming pub, outgoing msg to subs
Broker broker = new(ipAddress, 9000, 9050, 9090);
Broker.StartBroker();