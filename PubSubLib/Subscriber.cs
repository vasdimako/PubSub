using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PubSubLib
{
    public class Subscriber
    {
        private string ID { get; }
        private int IncMsgPort { get; }
        public IPAddress BrokerIP { get; set; }
        private int BrokerPort { get; }
        public string[] CommandList { get; set; }
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
                            IncMsgPort = Int32.Parse(args[i + 1]);
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
        public void Subscribe()
        {
            IPEndPoint localIP = new(BrokerIP, IncMsgPort);
            IPEndPoint endIP = new(BrokerIP, BrokerPort);
            Socket subscriber = new(BrokerIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            subscriber.Bind(localIP);

            try
            {
                subscriber.Connect(endIP);
                Console.WriteLine("Connected to {0}", endIP.ToString());
                Thread receiver = new(() => TryReceive(subscriber));
                receiver.Start();

                foreach (string command in CommandList)
                {
                    SubCommand(subscriber, command);
                }

                while (true)
                {
                    Console.WriteLine("Input wait time, sub/unsub and topic: ");
                    string input = Console.ReadLine();
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
        }
        private void SubCommand(Socket subscriber, string command)
        {
            Console.WriteLine(command);
            string[] commands = command.Split(" ");
            Thread.Sleep(int.Parse(commands[0])*1000);
            byte[] bytes = new byte[1024];

            // Encode the string into bytes.
            byte[] msg = Encoding.ASCII.GetBytes(ID + " " + commands[1] + " " + commands[2]);

            // Send the bytes.
            subscriber.Send(msg);
        }
        private void TryReceive(Socket subscriber)
        {
            while (true)
            {
                byte[] bytes = new byte[1024];
                int bytesRec = subscriber.Receive(bytes);
                Console.WriteLine("{0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));
            }
        }
    }
}