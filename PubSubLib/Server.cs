using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PubSubLib
{
    public class Server
    {
        public static string data = null;
        private static string[] SubInfo { get; set; } = new string[2];
        private static string[] Topics { get; set; } = new string[2];
        private static void SubThread(IPInfo ip)
        {
            byte[] bytes = new Byte[1024];
            IPEndPoint subEndIP = new(ip.Host, ip.SubPort);
            Socket subListener = new(ip.Host.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                subListener.Bind(subEndIP);
                subListener.Listen(10);

                // Start listening for connections.
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    Console.WriteLine(subEndIP.ToString());
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = subListener.Accept();

                    // An incoming connection needs to be processed.  
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    string[] lines = data.Split(
                        new string[] { "\n" }, StringSplitOptions.None);
                    SubInfo[0] = lines[0];
                    SubInfo[1] = lines[1];

                    // Show the data on the console.  
                    Console.WriteLine("ID: {0}, Topc: {1}", SubInfo[0], SubInfo[1]);

                    // Echo the data back to the client.  
                    byte[] msg = Encoding.ASCII.GetBytes("Subscribed to topic: " + SubInfo[1]);
                    handler.Send(msg);
                    while (true)
                    {
                        if (SubInfo[0] == Topics[0])
                        {
                            msg = Encoding.ASCII.GetBytes(Topics[1] + " -- Message: " + Topics[1]);
                            handler.Send(msg);
                            break;
                        }
                    }
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
        private static void PubThread(IPInfo ip)
        {
            byte[] bytes = new Byte[1024];
            IPEndPoint pubEndIP = new(ip.Host, ip.PubPort);
            Socket publistener = new(ip.Host.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                publistener.Bind(pubEndIP);
                publistener.Listen(10);
                // Start listening for connections.
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    Console.WriteLine(pubEndIP.ToString());
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = publistener.Accept();

                    // An incoming connection needs to be processed.  
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    string[] lines = data.Split(
                        new string[] { "\n" }, StringSplitOptions.None);
                    Topics[0] = lines[0];
                    Topics[1] = lines[1];

                    // Show the data on the console.  
                    Console.WriteLine("Topic: {0}, Message: {1}", Topics[0], Topics[1]);

                    // Echo the data back to the client.  
                    byte[] msg = Encoding.ASCII.GetBytes("Received message:" + Topics[1] + "(" + Topics[0] + ")");

                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public static void StartBroker(IPInfo ip)
        {
            Thread pub = new Thread(() => PubThread(ip));
            Thread sub = new Thread(() => SubThread(ip));
            pub.Start();
            sub.Start();

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }
    }
}
