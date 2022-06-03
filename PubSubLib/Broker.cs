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
    /// <summary>
    /// This is the broker class. After creating an object and specifying 
    /// the various arguments it must be launched with the <c>StartBroker</c> method.
    /// </summary>
    public class Broker
    {
        /// <summary>
        /// IP address is set to 127.0.0.1.
        /// </summary>
        private static IPAddress IP { get; set; }
        /// <summary>
        /// Specified by the -s argument, port to listen for subscriber connections.
        /// </summary>
        private static int SubPort { get; set; }
        /// <summary>
        /// Specified by the -p argument, port to listen for publisher connections.
        /// </summary>
        private static int PubPort { get; set; }
        /// <summary>
        /// Dictionary with sub IDs as the keys and Sockets of the connection between
        /// broker and the sub with that ID. Concurrent so that it is thread-safe.
        /// Initially uses a temporary key which is replaced when the first message is received.
        /// </summary>
        private static ConcurrentDictionary<string, Socket> SubHandlers { get; set; } = new();
        /// <summary>
        /// Dictionary with pub IDs as the keys and Sockets of the connection between
        /// broker and the pub with that ID. Concurrent so that it is thread-safe.
        /// Initially uses a temporary key which is replaced when the first message is received.
        /// </summary>
        private static ConcurrentDictionary<string, Socket> PubHandlers { get; set; } = new();
        /// <summary>
        /// Concurrent Dictionary with sub ID as the key and ConcurrentBag (List) of strings as the value.
        /// Topics each sub is subscribed to are stored here.
        /// </summary>
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> Topics { get; set; } = new();
        /// <summary>
        /// Constructs the broker object.
        /// </summary>
        /// <param name="args">Takes specified CLI arguments in a string array 
        /// and parses them into the correct broker properties.</param>
        public Broker(string[] args)
        {
            IP = IPAddress.Parse("127.0.0.1");

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case string s when s.StartsWith("-s"):
                        {
                            SubPort = Int32.Parse(args[i + 1]);
                            break;
                        }
                    case string s when s.StartsWith("-p"):
                        {
                            PubPort = Int32.Parse(args[i + 1]);
                            break;
                        }
                }
            }
        }
               /// <summary>
        /// Listen for subscribers and add the Socket connection to SubHandlers.
        /// </summary>
        /// <param name="token">Cancellation token for exiting out of thread.</param>
        private static void SubThread(CancellationToken token)
        {
            byte[] bytes = new Byte[1024];
            IPEndPoint subEndIP = new(IP, SubPort);
            Socket subListener = new(IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                subListener.Bind(subEndIP);
                Console.WriteLine(subEndIP.ToString());
                subListener.Listen(10);

                Thread close = new(() => CloseSocket(subListener, token));
                close.Start();

                // Start listening for connections.
                int temp = 0;
                while (true)
                {
                    string data = null;
                    // Program is suspended while waiting for an incoming connection.
                    Socket temphandler = subListener.Accept();
                    SubHandlers.TryAdd(temp.ToString(), temphandler);
                    Console.WriteLine("Connected to sub at " + temphandler.RemoteEndPoint.ToString());
                    // An incoming connection needs to be processed.  
                    temp++;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Subscriber socket closed.");
            }

        }
        /// <summary>
        /// Loops through all sub handlers and receives data if it is available. Parses new commands.
        /// If a command from a handler with a temporary name is passed, that handler is renamed to the sub ID.
        /// </summary>
        /// <param name="token">For thread cancellation.</param>
        private static void LoopSubHandlers(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    // If cancellation is requested, close all handler Sockets and exit thread.
                    foreach (KeyValuePair<string, Socket> handler in SubHandlers)
                        handler.Value.Close();
                    Console.WriteLine("SubHandler sockets and thread closed.");
                    break;
                }
                foreach (KeyValuePair<string, Socket> handler in SubHandlers)
                {
                    byte[] bytes = new Byte[1024];
                    string data = null;
                    if (handler.Value.Available > 0)
                    {
                        int bytesRec = handler.Value.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        Console.WriteLine(data);

                        string subID = ParseSubCommand(data, handler.Value);

                        if (!handler.Key.StartsWith("s"))
                        {
                            // The temp handler key is replaced with the subscriber ID from the incoming message.
                            SubHandlers.TryAdd(subID, handler.Value);
                            SubHandlers.TryRemove(handler);
                        }

                    }
                }
            }
        }
        /// <summary>
        /// Listen for subscribers and add the Socket connection to PubHandlers.
        /// </summary>
        /// <param name="token">Cancellation token for exiting out of thread.</param>
        private static void PubThread(CancellationToken token)
        {
            byte[] bytes = new Byte[1024];
            IPEndPoint pubEndIP = new(IP, PubPort);
            Socket pubListener = new(IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                pubListener.Bind(pubEndIP);
                Console.WriteLine(pubEndIP.ToString());
                pubListener.Listen(10);

                Thread close = new(() => CloseSocket(pubListener, token));
                close.Start();

                // Start listening for connections.
                int temp = 0;
                while (true)
                {
                    string data = null;
                    // Program is suspended while waiting for an incoming connection.
                    Socket temphandler = pubListener.Accept();
                    PubHandlers.TryAdd(temp.ToString(), temphandler);
                    Console.WriteLine("Connected to pub at " + temphandler.RemoteEndPoint.ToString());
                    // An incoming connection needs to be processed.  
                    temp++;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Publisher socket closed.");
            }

        }
        /// <summary>
        /// Loops through all pub handlers and receives data if it is available. Parses new commands.
        /// If a command from a handler with a temporary name is passed, that handler is renamed to the pub ID.
        /// </summary>
        /// <param name="token">For thread cancellation.</param>
        private static void LoopPubHandlers(CancellationToken token)
        {
            byte[] bytes = new Byte[1024];
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    // If cancellation is requested, close all handler Sockets and exit thread.
                    foreach (KeyValuePair<string, Socket> handler in PubHandlers)
                        handler.Value.Close();
                    Console.WriteLine("PubHandler sockets and thread closed.");
                    break;
                }
                foreach (KeyValuePair<string, Socket> handler in PubHandlers)
                {
                    if (handler.Value.Available > 0)
                    {
                        string data = null;
                        int bytesRec = handler.Value.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        Console.WriteLine(data);

                        string pubID =  ParsePubCommand(data, handler.Value);
                        
                        if (!handler.Key.StartsWith("p"))
                        {
                            // The temp handler key is replaced with the publisher ID from the incoming message.
                            PubHandlers.TryAdd(pubID, handler.Value);
                            PubHandlers.TryRemove(handler);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Parse command text, add or remove topic from SubInfo and send OK message.
        /// </summary>
        /// <param name="line">Command.</param>
        /// /// <param name="handler">Socket corresponding to the subscriber.</param>
        private static string ParseSubCommand(string line, Socket handler)
        {

            string[] args = line.Split(" ");
            switch (args[1])
            {
                case string s when String.Equals(s, "sub"):
                    {
                        if (Topics.ContainsKey(args[2]))
                        {
                            Topics[args[2]].TryAdd(args[0], true);
                        }
                        else
                        {
                            ConcurrentDictionary<string, bool> n = new();
                            n.TryAdd(args[0], true);
                            Topics.TryAdd(args[2], n);
                        }

                        // Show the data on the console.  
                        Console.WriteLine("ID: {0}, Topic: {1}", args[0], args[2]);

                        // Echo the data back to the client.  
                        byte[] msg = Encoding.ASCII.GetBytes("OK");

                        handler.Send(msg);

                        break;
                    }
                case string s when String.Equals(s, "unsub"):
                    {
                        Topics[args[2]].TryUpdate(args[0], false, true);
                        // Send unsub message.
                        Console.WriteLine("ID: {0} unsubbed from topic {1}", args[0], args[2]);
                        byte[] msg = Encoding.ASCII.GetBytes("OK");

                        handler.Send(msg);

                        break;
                    }
            }
            return args[0];
        }
        /// <summary>
        /// Parse command text, loop through SubInfo dictionary and send message through
        /// the appropriate subhandlers. Then send OK response.
        /// </summary>
        /// <param name="command">Command string</param>
        /// <param name="handler">Publisher handler.</param>
        /// <returns></returns>
        public static string ParsePubCommand(string command, Socket handler)
        {
            string[] process = command.Split(" ");
            string[] commands = process[0..3];

            string message = String.Join(" ", process[3..]);

            if (Topics.ContainsKey(commands[2]))
            {
                foreach (KeyValuePair<string, bool> sub in Topics[commands[2]])
                {
                    bool t;
                    if (Topics[commands[2]].TryGetValue(sub.Key, out t))
                    {
                        if (t)
                        {
                            byte[] msg = Encoding.ASCII.GetBytes("Received msg for topic " + commands[2] + ": " + message);
                            SubHandlers[sub.Key].Send(msg);
                        }
                    }
                }
            }
            byte[] resp = Encoding.ASCII.GetBytes("OK");
            handler.Send(resp);
            return commands[0];
        }
        /// <summary>
        /// Thread to close sockets waiting in the .Accept() method.
        /// </summary>
        /// <param name="handler">Socket to be closed.</param>
        /// <param name="token">Cancellation token.</param>
        private static void CloseSocket(Socket handler, CancellationToken token) {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    handler.Close();
                    break;
                }
            }
        }
        /// <summary>
        /// Start all broker threads. Accepts the exit CLI command.
        /// </summary>
        public void StartBroker()
        {
            CancellationTokenSource cancelToken = new();
            
            Thread sub = new(() => SubThread(cancelToken.Token));
            Thread subHandlerThread = new(() => LoopSubHandlers(cancelToken.Token));
            subHandlerThread.Start();
            sub.Start();

            Thread pub = new(() => PubThread(cancelToken.Token));
            Thread pubHandlerThread = new(() => LoopPubHandlers(cancelToken.Token));
            pub.Start();
            pubHandlerThread.Start();

            string input = Console.ReadLine();
            if (input.Equals("exit"))
            {
                Console.WriteLine("Closing threads...");
                cancelToken.Cancel();
                Thread.Sleep(1500);
                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
            }
        }
    }
}