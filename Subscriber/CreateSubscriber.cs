using PubSubLib;

string[] clArgs;

if (Environment.GetCommandLineArgs().Length <= 1)
{
    clArgs = "subscriber -i s1 -r 9090 -h 127.0.0.1 -p 9000 -f subscriber1.cmd".Split();
}
else
{
    clArgs = Environment.GetCommandLineArgs()[1..];
}

Subscriber sub = new(clArgs);
sub.Subscribe();