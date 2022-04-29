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

         public void Publish()
        {
            foreach (string command in CommandList)
            {
                PubSend(command);
            }
            while (true)
            {
                Console.WriteLine("Input wait time, pub, topic and message:");
                string input = Console.ReadLine();
                PubSend(input);
            }

        }
        private void PubSend(string command)
        {
            // Create a socket of a certain type.
            Socket publisher = new(BrokerIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endIP = new(BrokerIP, BrokerPort);
            // Create byte buffer.
            byte[] bytes = new byte[1024];

            // Parse command, returning command terms and message in a tuple.
            Tuple<string[], string> t = ParseCommand(command);
            string[] commands = t.Item1;
            string message = t.Item2;

            // Wait for number of seconds specified in command.
            int wait = Int32.Parse(commands[0]);
            Thread.Sleep(1000 * wait);

            try
            {
                publisher.Connect(endIP);
                Console.WriteLine("Connected to {0}", publisher.RemoteEndPoint.ToString());
                string data = ID + " " + commands[1] + " " + commands[2] + " " + message;
                // Encode the string into bytes.
                byte[] msg = Encoding.ASCII.GetBytes(data);

                // Send the bytes.
                int bytesSent = publisher.Send(msg);

                // Receive response from remote device.
                int bytesRec = publisher.Receive(bytes);
                Console.WriteLine("{0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));
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
        public static Tuple<string[], string> ParseCommand(string command)
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
    }

}