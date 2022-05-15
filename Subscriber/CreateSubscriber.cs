using PubSubLib;
using System.Net;


Subscriber sub = new Subscriber("subscriber -i s1 -r 9090 -h 127.0.0.1 -p 9000 -f subscriber1.cmd");
Subscriber sub2 = new Subscriber("subscriber -i s2 -r 9100 -h 127.0.0.1 -p 9000 -f subscriber1.cmd");

sub.Subscribe();

sub2.Subscribe();