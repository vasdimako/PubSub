using PubSubLib;
using System.Net;

Publisher pub = new("publisher -i p1 -r 8200 -h 127.0.0.1 -p 9050 -f publisher1.cmd");
Thread.Sleep(10000);
pub.Publish("3 pub #hello This is the first message");
Console.ReadLine();