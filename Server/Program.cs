using PubSubLib;
using System.Net;

IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
IPAddress ipAddress = ipHostInfo.AddressList[0];

IPInfo ip = new(ipAddress, 9000, 9090);

Broker.StartBroker(ip);