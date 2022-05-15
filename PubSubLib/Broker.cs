using System;
using System.Collections;
using System.Collections.Concurrent;
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
        private static ConcurrentDictionary<string, Socket> SubHandlers { get; set; } = new();
        private static ConcurrentDictionary<string, Socket> PubHandlers { get; set; } = new();
        public Broker(string run)
        {
            IP = IPAddress.Parse("127.0.0.1");

            string[] args = run.Split('-');
            foreach (string arg in args)
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
        private static ConcurrentDictionary<string, ConcurrentBag<string>> SubInfo { get; set; } = new ConcurrentDictionary<string, ConcurrentBag<string>>();
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

                Thread handlerThread = new(LoopSubHandlers);
                handlerThread.Start();

                // Start listening for connections.
                int temp = 0;
                while (true)
                {
                    string data = null;
                    Console.WriteLine("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.
                    SubHandlers.TryAdd(temp.ToString(), subListener.Accept());
                    Console.WriteLine("Connected to " + subEndIP.ToString());
                    // An incoming connection needs to be processed.  
                    temp++;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
        private static void LoopSubHandlers()
        {
            while (true)
            {
                foreach (KeyValuePair<string, Socket> handler in SubHandlers)
                {
                    byte[] bytes = new Byte[1024];
                    string data = null;
                    if (handler.Value.Available > 0)
                    {
                        int bytesRec = handler.Value.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        Console.WriteLine(data);
                        string[] lines = data.Split(" ");

                        ParseSubCommand(lines, handler.Value);

                        if (!handler.Key.StartsWith("s"))
                        {
                            SubHandlers.TryAdd(lines[0], handler.Value);
                            SubHandlers.TryRemove(handler);
                        }

                    }

                    //SubInfo.TryGetValue(handler.Key, out List<string> topics);
                    //if (topics != null)
                    //{
                    //    foreach (string topic in topics)
                    //    {
                    //        Messages.TryGetValue(topic, out List<string> messages);
                    //        if (messages != null)
                    //        {
                    //            foreach (string message in messages)
                    //            {
                    //                byte[] msg = Encoding.ASCII.GetBytes(message);
                    //                handler.Value.Send(msg);
                    //            }
                    //        }
                    //    }
                    //}

                    //Something about checking publishers.
                }
            }
        }

        private static void PubThread()
        {
            byte[] bytes = new Byte[1024];
            IPEndPoint pubEndIP = new(IP, PubPort);
            Socket pubListener = new(IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                pubListener.Bind(pubEndIP);
                Console.WriteLine(pubEndIP.ToString());
                pubListener.Listen(10);

                Thread handlerThread = new(LoopPubHandlers);
                handlerThread.Start();

                // Start listening for connections.
                int temp = 0;
                while (true)
                {
                    string data = null;
                    Console.WriteLine("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.
                    PubHandlers.TryAdd(temp.ToString(), pubListener.Accept());
                    Console.WriteLine("Connected to " + pubEndIP.ToString());
                    // An incoming connection needs to be processed.  
                    temp++;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
        private static void LoopPubHandlers()
        {
            byte[] bytes = new Byte[1024];
            while (true)
            {
                foreach (KeyValuePair<string, Socket> handler in PubHandlers)
                {
                    if (handler.Value.Available > 0)
                    {
                        string data = null;
                        int bytesRec = handler.Value.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        Console.WriteLine(data);

                        string pubname =  ParsePubCommand(data, handler.Value);
                        
                        if (!handler.Key.StartsWith("p"))
                        {
                            PubHandlers.TryAdd(pubname, handler.Value);
                            PubHandlers.TryRemove(handler);
                        }
                    }

                    //PubInfo.TryGetValue(handler.Key, out List<string> topics);
                    //if (topics != null)
                    //{
                    //    foreach (string topic in topics)
                    //    {
                    //        Messages.TryGetValue(topic, out List<string> messages);
                    //        if (messages != null)
                    //        {
                    //            foreach (string message in messages)
                    //            {
                    //                byte[] msg = Encoding.ASCII.GetBytes(message);
                    //                handler.Value.Send(msg);
                    //            }
                    //        }
                    //    }
                    //}

                    ////Something about checking publishers.
                }
            }
        }
        private static void ParseSubCommand(string[] args, Socket handler)
        {
            switch (args[1])
            {
                case string s when String.Equals(s, "sub"):
                    {
                        if (SubInfo.ContainsKey(args[0]))
                        {
                            SubInfo[args[0]].Add(args[2]);
                        }
                        else
                        {
                            SubInfo.TryAdd(args[0], new ConcurrentBag<string> { args[2] });
                        }

                        // Show the data on the console.  
                        Console.WriteLine("ID: {0}, Topic: {1}", args[0], args[2]);

                        // Echo the data back to the client.  
                        byte[] msg = Encoding.ASCII.GetBytes("subbed to topic: " + args[2]);

                        handler.Send(msg);


                        break;
                    }
                case string s when String.Equals(s, "unsub"):
                    {
                        ConcurrentBag<string> delval = SubInfo[args[0]];
                        SubInfo.TryRemove(args[0], out delval);
                        // Send unsub message.
                        Console.WriteLine("ID: {0} unsubbed from topic {1}", args[0], args[2]);
                        byte[] msg = Encoding.ASCII.GetBytes("unsub from " + args[2]);

                        handler.Send(msg);

                        break;
                    }
            }
        }
        public static string ParsePubCommand(string command, Socket handler)
        {
            string[] process = command.Split(" ");
            string[] commands = process[0..3];

            string message = null;
            foreach (string msgbit in process[3..])
            {
                message += msgbit;
                message += " ";
            }
            message = message.TrimEnd();

            foreach (KeyValuePair<string, ConcurrentBag<string>> sub in SubInfo) 
            {
                if (sub.Value.Contains(commands[2]))
                {
                    byte[] msg = Encoding.ASCII.GetBytes("Received message: " + message + "(" + commands[2] + ")");
                    SubHandlers[sub.Key].Send(msg);
                }
            }
            //if (Messages.ContainsKey(commands[2]))
            //{
            //    Messages[commands[2]].Add(message);
            //} else
            //{
            //    List<string> messagelist = new();
            //    messagelist.Add(message);
            //    Messages.TryAdd(commands[2], messagelist);
            //}
            byte[] resp = Encoding.ASCII.GetBytes("Received message: " + message + "(" + commands[2] + ")");
            handler.Send(resp);
            return commands[0];
        }
        public void StartBroker()
        {
            Thread pub = new(PubThread);
            Thread sub = new(SubThread);
            pub.Start();
            sub.Start();
        }
    }
}