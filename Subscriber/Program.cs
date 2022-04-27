using PubSubLib;
using System.Net;

IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
IPAddress ipAddress = ipHostInfo.AddressList[0];

IPInfo ip = new(ipAddress, 9000, 9090);

Subscriber sub = new Subscriber("subscriber -i s1 -r 9000 -h 127.0.0.1 -p 9090 -f subscriber1.cmd");

Thread command1 = new(() => sub.SubExecute("s1 sub #topic"));
Thread command2 = new(() => sub.SubExecute("s1 unsub #topic"));

command1.Start();
Thread.Sleep(2000);
command2.Start();
Console.Read();