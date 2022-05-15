﻿using PubSubLib;
using System.Net;

string clArgs;

if (Environment.GetCommandLineArgs().Length <= 1)
{
    string[] clArgs = "-s 9000 -p 9050".Split(" ");
}

else
{
    clArgs = Environment.GetCommandLineArgs()[1];
}

// ip, subport, pubport, msgport - incoming sub, incoming pub, outgoing msg to subs
Broker broker = new Broker(clArgs);

broker.StartBroker();