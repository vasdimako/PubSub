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
        private int Port { get; }
        public IPAddress BrokerIP { get; set; }
        private int BrokerPort { get; }
        private string[] CommandList { get; set; }
        public string[] ParseCommand(string command)
        {
            return command.Split(" ");
        }
        public string GetSubInfo()
        {
            return (ID + "\n" + Port.ToString() + "\n" + BrokerIP.ToString() + "\n" + BrokerPort.ToString() + "\n" + CommandList.ToString());
        }
        public Subscriber(string run)
        {
            string[] args = run.Split('-');
            foreach(string arg in args)
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
                            Port = Int32.Parse(arg.Substring(2).Trim());
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
        public void SubExecute(string command)
        {
            // Create a socket of a certain type.
            Socket subscriber = new(BrokerIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endIP = new(BrokerIP, BrokerPort);
            // Create byte buffer.
            byte[] bytes = new byte[1024];
            string[] commands = command.Split(" ");
            try
            {
                Thread.Sleep(1000);
                subscriber.Connect(endIP);
                Console.WriteLine("Connected to {0}", subscriber.RemoteEndPoint.ToString());

                // Check the command and either sub and continue with the code, or unsub and end method 
                if (String.Equals(commands[1], "unsub"))
                {
                    // Encode the string into bytes.
                    byte[] msg = Encoding.ASCII.GetBytes(ID + " " + commands[1] + " " + commands[2]);
                    Console.WriteLine(msg.ToString());

                    // Send the bytes.
                    int bytesSent = subscriber.Send(msg);

                    // Receive response from remote device.
                    int bytesRec = subscriber.Receive(bytes);
                    Console.WriteLine("{0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    //Release socket.
                    subscriber.Shutdown(SocketShutdown.Both);
                    subscriber.Close();
                    return; // This ends the method

                } 
                else if (String.Equals(commands[1], "sub"))
                {
                    // Encode the string into bytes.
                    byte[] msg = Encoding.ASCII.GetBytes(ID + " " + commands[1] + " " + commands[2]);
                    Console.WriteLine(msg.ToString());

                    // Send the bytes.
                    int bytesSent = subscriber.Send(msg);

                    // Receive response from remote device.
                    int bytesRec = subscriber.Receive(bytes);
                    Console.WriteLine("{0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    //Release socket.
                    subscriber.Shutdown(SocketShutdown.Both);
                    subscriber.Close();
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
            try
            {
                // Create response socket - note this only happens if command[1] is sub
                Socket subResponse = new(BrokerIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint subReEndIP = new(BrokerIP, Port);

                subResponse.Bind(subReEndIP);
                subResponse.Listen(10);

                // Start listening for connections.
                while (true)
                {
                    string resp = null;
                    Console.WriteLine("Waiting for a connection...");
                    Console.WriteLine(subReEndIP.ToString());
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = subResponse.Accept();

                    // An incoming connection needs to be processed.  
                    int bytesRec = handler.Receive(bytes);
                    resp += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    Console.WriteLine(resp);
                    string[] respl = resp.Split(" ");
                    if (String.Equals(respl[0], "unsub"))
                    {
                        Console.WriteLine(resp);
                        Console.ReadLine();
                        return;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
    }
}