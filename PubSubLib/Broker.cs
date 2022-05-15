using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PubSubLib
{
    
    public class Broker
    {
        private static IPAddress IP { get; set; }
        private static int SubPort { get; set; }
        private static int PubPort { get; set; }
        public Broker(string[] clArgs)
        {
            IP = IPAddress.Parse("127.0.0.1");

            foreach (string arg in clArgs)
            {
                switch (arg)
                {
                    case string s when s.StartsWith("s "):
                        {
                            SubPort = Int32.Parse(arg.Substring(2).Trim());
                            break;
                        }
                    case string s when s.StartsWith("p "):
                        {
                            PubPort = Int32.Parse(arg.Substring(2).Trim());
                            break;
                        }
                }
            }
        }
        private static Dictionary<string, List<string>> SubInfo { get; set; } = new Dictionary<string, List<string>>();
        private static void SubThread()
        {
            byte[] bytes = new Byte[1024];
            IPEndPoint subEndIP = new(IP, SubPort);
            Socket subListener = new(IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                subListener.Bind(subEndIP);
                Console.WriteLine(subEndIP.ToString());
                subListener.Listen(10);

                // Start listening for connections.
                while (true)
                {
                    string data = null;
                    Console.WriteLine("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = subListener.Accept();
                    Console.WriteLine("Connected to " + subEndIP.ToString());
                    // An incoming connection needs to be processed.  
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    Console.WriteLine(data);
                    string[] lines = data.Split(" ");

                    byte[] msg = ParseSubCommand(lines);

                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
 
        private static void PubThread()
        {
            byte[] bytes = new Byte[1024];
            IPEndPoint pubEndIP = new(IP, PubPort);
            Socket publistener = new(IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                publistener.Bind(pubEndIP);
                Console.WriteLine(pubEndIP.ToString());
                publistener.Listen(10);
                // Start listening for connections.
                while (true)
                {
                    string data = null;
                    Console.WriteLine("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = publistener.Accept();
                    Console.WriteLine(pubEndIP.ToString());
                    // An incoming connection needs to be processed.  
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    // Parse the data, separating the command arguments and message.
                    Tuple<string[], string> t = ParsePubCommand(data);
                    string[] command = t.Item1;
                    string message = t.Item2;

                    // Show the data on the console.  
                    Console.WriteLine(data);

                    // Echo the data back to the client.  
                    byte[] msg = Encoding.ASCII.GetBytes("Received message: " + message + "(" + command[2] + ")");
                    handler.Send(msg);

                    List<string> sublist = CheckSubs(command[2]);

                    if (sublist.Count > 0)
                    {
                        foreach (string sub in sublist)
                        {
                            int subPort = Int32.Parse(SubInfo[sub][0]);
                            IPEndPoint subEndIP = new(IP, subPort);
                            Socket subsender = new(IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                            Console.WriteLine("Subscriber IP: " + subEndIP.ToString());
                            subsender.Connect(subEndIP);
                            msg = Encoding.ASCII.GetBytes(message + "(" + command[2] + ")");
                            Console.WriteLine("Sending message to subscriber");
                            subsender.Send(msg);
                        }
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private static byte[] ParseSubCommand(string[] args)
        {
            switch (args[1])
            {
                case String s when String.Equals(s, "meta"):
                    {
                        SubInfo.Add(args[0], new List<string> { args[2] });

                        // Show the data on the console.  
                        Console.WriteLine("ID: {0}, Port: {1}", args[0], args[2]);

                        // Echo the data back to the client.  
                        byte[] msg = Encoding.ASCII.GetBytes("ID: " + args[0] + " " + "Port: " + args[2]);

                        return msg;
                    }
                case string s when String.Equals(s, "sub"):
                    {
                        if (SubInfo.ContainsKey(args[0]))
                        {
                            SubInfo[args[0]].Add(args[2]);
                        }

                        // Show the data on the console.  
                        Console.WriteLine("ID: {0}, Topic: {1}", args[0], args[2]);

                        // Echo the data back to the client.  
                        byte[] msg = Encoding.ASCII.GetBytes("subbed to topic: " + args[2]);

                        return msg;
                    }
                case string s when String.Equals(s, "unsub"):
                    {
                        SubInfo.Remove(args[0]);
                        // Send unsub message.
                        Console.WriteLine("ID: {0} unsubbed from topic {1}", args[0], args[2]);
                        byte[] msg = Encoding.ASCII.GetBytes("unsub from " + args[2]);

                        return msg;
                    }
            }
            return null;
        }

        public static Tuple<string[], string> ParsePubCommand(string command)
        {
            string[] process = command.Split(" ");
            string[] commands = process[0..3];

            string message = null;
            foreach (string msgbit in process[3..])
            {
                message += msgbit;
                message += " ";
            }
            message.TrimEnd();

            return Tuple.Create(commands, message);
        }
        private static List<string> CheckSubs(string topic)
        {
            var sublist = new List<string> { };

            foreach (string sub in SubInfo.Keys)
            {
                if (SubInfo[sub].Contains(topic))
                {
                    sublist.Add(sub);
                }
            }
            return sublist;
        }
        public static void StartBroker()
        {
            Thread pub = new(PubThread);
            Thread sub = new(SubThread);
            pub.Start();
            sub.Start();
        }
    }
}