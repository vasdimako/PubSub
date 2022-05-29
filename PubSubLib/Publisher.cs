using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PubSubLib
{
    public class Publisher
    {
        private string ID { get; set; }
        private int PubPort { get; set; }
        private IPAddress BrokerIP { get; set; }
        private int BrokerPort { get; set; }
        public string[] CommandList { get; set; }
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

        public void Publish()
        {
            IPEndPoint localIP = new(BrokerIP, PubPort);
            IPEndPoint endIP = new(BrokerIP, BrokerPort);
            Socket publisher = new(BrokerIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            publisher.Bind(localIP);

            try
            {
                publisher.Connect(endIP);
                Console.WriteLine("Connected to {0}", endIP.ToString());

                foreach (string command in CommandList)
                {
                    PubCommand(publisher, command);
                }
                while (true)
                {
                    Console.WriteLine("Input wait time, pub, topic and message:");
                    string input = Console.ReadLine();
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
        }
        private void PubCommand(Socket publisher, string command)
        {
            byte[] bytes = new byte[1024];
            (string[], string) t = ParseCommand(command);
            string[] commands = t.Item1;
            string message = t.Item2;

            // Wait for number of seconds specified in command.
            int wait = Int32.Parse(commands[0]);
            Thread.Sleep(1000 * wait);

            string data = ID + " " + commands[1] + " " + commands[2] + " " + message;
            Console.WriteLine(data);
            // Encode the string into bytes.
            byte[] msg = Encoding.ASCII.GetBytes(data);

            // Send the bytes.
            int bytesSent = publisher.Send(msg);

            // Receive response from remote device.
            int bytesRec = publisher.Receive(bytes);
            Console.WriteLine("Published msg for topic " + commands[2] + ": " + message);
        }

        private static (string[], string) ParseCommand(string command)
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

            return (commands, message);
        }
    }
}