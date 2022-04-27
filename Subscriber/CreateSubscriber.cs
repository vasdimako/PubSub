using PubSubLib;
using System.Net;


Subscriber sub = new Subscriber("subscriber -i s1 -r 9090 -h 127.0.0.1 -p 9000 -f subscriber1.cmd");

Thread command1 = new(() => sub.Subscribe("s1 sub #hello"));
Thread command2 = new(() => sub.Subscribe("s1 unsub #topic"));

command1.Start();
Thread.Sleep(5000);
command2.Start();
Console.Read();