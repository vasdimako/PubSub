using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PubSubLib
{
    /// <summary>
    /// This is the publisher class. After creating an object and specifying 
    /// the various arguments it must be launched with the <c>Subscribe</c> method.
    /// </summary>
    public class Publisher
    {
        /// <summary>
        /// The -i argument, publisher ID.
        /// </summary>
        private string ID { get; set; }
        /// <summary>
        /// The -r argument, publisher port.
        /// </summary>
        private int PubPort { get; set; }
        /// <summary>
        /// The -h argument, broker IP.
        /// </summary>
        private IPAddress BrokerIP { get; set; }
        /// <summary>
        /// The -p argument, broker port.
        /// </summary>
        private int BrokerPort { get; set; }
        /// <summary>
        /// The -f argument parsed into a string array. <c>null</c> if no command file is passed.
        /// </summary>
        public string[]? CommandList { get; set; } = null;
        /// <summary>
        /// Constructs the publisher object.
        /// </summary>
        /// <param name="args">Takes specified CLI arguments in a string array 
        /// and parses them into the correct publisher properties.</param>
        public Publisher(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case string s when s.Equals("-i"):
                        {
                            ID = args[i + 1];
                            break;
                        }
                    case string s when s.Equals("-r"):
                        {
                            PubPort = Int32.Parse(args[i + 1]);
                            break;
                        }
                    case string s when s.Equals("-h"):
                        {
                            BrokerIP = IPAddress.Parse(args[i + 1]);
                            break;
                        }
                    case string s when s.Equals("-p"):
                        {
                            BrokerPort = Int32.Parse(args[i + 1]);
                            break;
                        }
                    case string s when s.Equals("-f"):
                        {
                            string filepath = "C:/Users/Vasilis/source/repos/PubSub/" + args[i + 1];
                            CommandList = File.ReadAllLines(filepath);
                            break;
                        }
                }
            }
        }
        /// <summary>
        /// Launch connection from publisher object to broker.
        /// </summary>
        public void Publish()
        {
            // Publisher IP and port.
            IPEndPoint localIP = new(BrokerIP, PubPort);
            // Broker IP and port.
            IPEndPoint endIP = new(BrokerIP, BrokerPort);
            Socket publisher = new(BrokerIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            publisher.Bind(localIP);

            try
            {
                publisher.Connect(endIP);
                Console.WriteLine("Connected to {0}", endIP.ToString());

                if (CommandList != null)
                {
                    // If a command file has been given,
                    // the command list will not be null.
                    foreach (string command in CommandList)
                    {
                        PubCommand(publisher, command);
                    }
                }
                Thread.Sleep(1000);
                while (true)
                {
                    Console.WriteLine("Input wait time, pub, topic and message:");
                    string input = Console.ReadLine();
                    if (input.Equals("exit"))
                    {
                        Console.WriteLine("Closing connection socket and exiting...");
                        publisher.Close();
                        break;
                    }
                    PubCommand(publisher, input);
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
        /// <param name="publisher">Socket connection between publisher and broker.</param>
        /// <param name="command">Single line of the command file or CLI input.</param>
        private void PubCommand(Socket publisher, string command)
        {
            byte[] bytes = new byte[1024];
            string[] process = command.Split(" ");
            // Wait for number of seconds specified in command.
            int wait = Int32.Parse(process[0]);
            Thread.Sleep(1000 * wait);

            string data = ID + " " + String.Join(" ", process[1..]);
            Console.WriteLine(data);
            // Encode the string into bytes.
            byte[] msg = Encoding.ASCII.GetBytes(data);

            // Send the bytes.
            int bytesSent = publisher.Send(msg);

            // Receive response from remote device.
            int bytesRec = publisher.Receive(bytes);
            Console.WriteLine("Published msg for topic {0}: {1}", process[2], String.Join(" ", process[3..]));
        }
    }
}