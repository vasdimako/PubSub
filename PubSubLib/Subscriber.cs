using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PubSubLib
{
    /// <summary>
    /// This is the subscriber class. After creating an object and specifying 
    /// the various arguments it must be launched with the <c>Subscribe</c> method.
    /// </summary>
    public class Subscriber
    {
        /// <summary>
        /// The -i argument, subscriber ID.
        /// </summary>
        private string ID { get; }
        /// <summary>
        /// The -r argument, subscriber port.
        /// </summary>
        private int SubPort { get; }
        /// <summary>
        /// The -h argument, broker IP.
        /// </summary>
        public IPAddress BrokerIP { get; set; }
        /// <summary>
        /// The -p argument, broker port.
        /// </summary>
        private int BrokerPort { get; }
        /// <summary>
        /// The -f argument, commands in a string array.
        /// </summary>
        public string[]? CommandList { get; set; } = null;
        /// <summary>
        /// Constructs the subscriber object.
        /// </summary>
        /// <param name="args">Takes specified CLI arguments in a string array 
        /// and parses them into the correct subscriber properties.</param>
        private bool CommandOK { get; set; } = false;
        public Subscriber(string[] args) {

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case string s when s.StartsWith("-i"):
                        {
                            ID = args[i + 1];
                            break;
                        }
                    case string s when s.StartsWith("-r"):
                        {
                            SubPort = Int32.Parse(args[i + 1]);
                            break;
                        }
                    case string s when s.StartsWith("-h"):
                        {
                            BrokerIP = IPAddress.Parse(args[i + 1]);
                            break;
                        }
                    case string s when s.StartsWith("-p"):
                        {
                            BrokerPort = Int32.Parse(args[i + 1]);
                            break;
                        }
                    case string s when s.StartsWith("-f"):
                        {
                            string filepath = "C:/Users/Vasilis/source/repos/PubSub/" + args[i + 1];
                            CommandList = File.ReadAllLines(filepath);
                            break;
                        }
                }
            }
        }
        /// <summary>
        /// Launch connection from subscriber object to broker.
        /// </summary>
        public void Subscribe()
        {
            // Subscriber IP and port.
            IPEndPoint localIP = new(BrokerIP, SubPort);
            // Broker IP and port.
            IPEndPoint endIP = new(BrokerIP, BrokerPort);
            Socket subscriber = new(BrokerIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            subscriber.Bind(localIP);

            try
            {
                subscriber.Connect(endIP);
                Console.WriteLine("Connected to {0}", endIP.ToString());

                CancellationTokenSource cancelToken = new();
                Thread receiver = new(() => TryReceive(subscriber, cancelToken.Token));
                receiver.Start();

                if (CommandList != null)
                {
                    // If a command file has been given,
                    // the command list will not be null.
                    foreach (string command in CommandList)
                    {
                        SubCommand(subscriber, command);
                    }
                }

                Thread.Sleep(1000);
                // Wait for input.
                while (true)
                {
                    Console.WriteLine("Input wait time, sub/unsub and topic: ");
                    string input = Console.ReadLine();
                    if (input.Equals("exit"))
                    {
                        // Pass cancellation token to TryReceive thread and exit command to SubCommand.
                        Console.WriteLine("Closing connection socket and exiting...");
                        cancelToken.Cancel();
                        Thread.Sleep(1500);
                        subscriber.Close();
                        break;
                    }
                    SubCommand(subscriber, input);
                }

            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
            Console.WriteLine("Press Enter to exit command line.");
            Console.ReadLine();
        }
        /// <summary>
        /// Parses a command and sends it to the broker through the specified socket.
        /// </summary>
        /// <param name="subscriber">Socket connection between subscriber and broker.</param>
        /// <param name="command">Single line of the command file or CLI input.</param>
        private void SubCommand(Socket subscriber, string command)
        {
            string[] commands = command.Split(" ");
            Thread.Sleep(int.Parse(commands[0]) * 1000);
            Console.WriteLine(command);
            byte[] bytes = new byte[1024];

            // Encode the string into bytes.
            byte[] msg = Encoding.ASCII.GetBytes(ID + " " + commands[1] + " " + commands[2]);

            // Send the bytes.
            subscriber.Send(msg);
            while (!CommandOK)
            {
                // Waits for TryReceive to receive an OK message before finishing with the command.
                Thread.Sleep(1000);
            }
            CommandOK = false;
        }
        /// <summary>
        /// Await messages from the broker at the specified socket.
        /// </summary>
        /// <param name="subscriber">Socket connection between subscriber and broker.</param>
        private void TryReceive(Socket subscriber, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    byte[] bytes = new byte[1024];
                    int bytesRec = subscriber.Receive(bytes);
                    string msg = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    Console.WriteLine(msg);
                    if (msg.Equals("OK"))
                    {
                        this.CommandOK = true;
                    }
                }
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("Socket successfully closed by host machine.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
        }
    }
}