using PubSubLib;
using System.Net;

string clArgs;

if (Environment.GetCommandLineArgs().Length <= 1)
{
    string[] clArgs = "-i p1 -r 8200 -h 127.0.0.1 -p 9050 -f publisher1.cmd".Split(" ");
} else
{
    clArgs = Environment.GetCommandLineArgs()[1];
}

// ip, subport, pubport, msgport - incoming sub, incoming pub, outgoing msg to subs
Publisher pub = new(clArgs);
pub.Publish();