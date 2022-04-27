using PubSubLib;
using System.Net;

IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
// IPAddress ipAddress = ipHostInfo.AddressList[0];
IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

IPInfo ip = new(ipAddress, 9090, 9100);

Broker.StartBroker(ip);