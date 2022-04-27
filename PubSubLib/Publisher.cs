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
        private string[] CommandList { get; set; }
        public Publisher(string runargs)
        {
            string[] args = runargs.Split('-');
            foreach (string arg in args)
            {
                Console.WriteLine(arg);
                switch (arg)
                {
                    case string s when s.StartsWith("i "):
                        {
                            ID = arg.Substring(2).Trim();
                            break;
                        }
                    case string s when s.StartsWith("r "):
                        {
                            PubPort = Int32.Parse(arg.Substring(2).Trim());
                            break;
                        }
                    case string s when s.StartsWith("h "):
                        {
                            BrokerIP = IPAddress.Parse(arg.Substring(2).Trim());
                            break;
                        }
                    case string s when s.StartsWith("p "):
                        {
                            BrokerPort = Int32.Parse(arg.Substring(2).Trim());
                            break;
                        }
                    case string s when s.StartsWith("f "):
                        {
                            string filepath = "C:/Users/Vasilis/source/repos/PubSub/" + arg.Substring(2).Trim();
                            CommandList = File.ReadAllLines(filepath);
                            break;
                        }
                }
            }
        }   
         public void Publish(string command)
        {
            // Create a socket of a certain type.
            Socket publisher = new(BrokerIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endIP = new(BrokerIP, BrokerPort);
            // Create byte buffer.
            byte[] bytes = new byte[1024];
            try
            {
                publisher.Connect(endIP);
                Console.WriteLine("Connected to {0}", publisher.RemoteEndPoint.ToString());
                string data = ID + " " + command.Substring(2);
                // Encode the string into bytes.
                byte[] msg = Encoding.ASCII.GetBytes(data);

                // Send the bytes.
                int bytesSent = publisher.Send(msg);

                // Receive response from remote device.
                int bytesRec = publisher.Receive(bytes);
                Console.WriteLine("{0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));
                Console.ReadLine();
                //Release socket.
                publisher.Shutdown(SocketShutdown.Both);
                publisher.Close();
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
    }
}