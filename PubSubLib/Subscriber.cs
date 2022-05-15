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
        public Subscriber(string run)
        {
            string[] args = run.Split('-');
            foreach (string arg in args)
            {
                Console.WriteLine(arg);
                switch (arg)
                {
                    case string s when s.StartsWith("i"):
                        {
                            ID = arg.Substring(2).Trim();
                            break;
                        }
                    case string s when s.StartsWith("r"):
                        {
                            IncMsgPort = Int32.Parse(arg.Substring(2).Trim());
                            break;
                        }
                    case string s when s.StartsWith("h"):
                        {
                            BrokerIP = IPAddress.Parse(arg.Substring(2).Trim());
                            break;
                        }
                    case string s when s.StartsWith("p"):
                        {
                            BrokerPort = Int32.Parse(arg.Substring(2).Trim());
                            break;
                        }
                    case string s when s.StartsWith("f"):
                        {
                            string filepath = "C:/Users/Vasilis/source/repos/PubSub/" + arg.Substring(2).Trim();
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
                    Console.WriteLine(command);
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
    //private void SubSend(string command)
    //{
    //    string[] commands = command.Split(" ");

    //    int wait = Int32.Parse(commands[0]);
    //    Thread.Sleep(1000 * wait);

    //    // Create a socket of a certain type.
    //    Socket subscriber = new(BrokerIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    //    IPEndPoint endIP = new(BrokerIP, BrokerPort);
    //    // Create byte buffer.
    //    byte[] bytes = new byte[1024];

    //    try
    //    {
    //        subscriber.Connect(endIP);
    //        Console.WriteLine("Connected to {0}", subscriber.RemoteEndPoint.ToString());

    //        // Check the command and either sub and continue with the code, or unsub and end method 


    //    }
    //    catch (ArgumentNullException ane)
    //    {
    //        Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
    //    }
    //    catch (SocketException se)
    //    {
    //        Console.WriteLine("SocketException : {0}", se.ToString());
    //    }
    //    catch (Exception e)
    //    {
    //        Console.WriteLine("Unexpected exception : {0}", e.ToString());
    //    }
    //}
    //private void SubListen()
    //{
    //    try
    //    {
    //        // Create response socket - note this only happens if command[1] is sub
    //        Socket subResponse = new(BrokerIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    //        IPEndPoint subReEndIP = new(BrokerIP, IncMsgPort);

    //        subResponse.Bind(subReEndIP);
    //        subResponse.Listen(10);

    //        while (true)
    //        {
    //            string resp = null;
    //            byte[] bytes = new byte[1024];
    //            Console.WriteLine("Waiting for a connection...");
    //            // Program is suspended while waiting for an incoming connection.  
    //            Socket handler = subResponse.Accept();

    //            // An incoming connection needs to be processed.  
    //            int bytesRec = handler.Receive(bytes);
    //            resp += Encoding.ASCII.GetString(bytes, 0, bytesRec);
    //            Console.WriteLine(resp);
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        Console.WriteLine(e.ToString());
    //    }

    //    }
}