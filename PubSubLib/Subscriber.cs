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
        public static void Subscribe(string data, IPInfo ip)
        {
            // Create a socket of a certain type.
            Socket publisher = new(ip.Host.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endIP = new(ip.Host, ip.Port);
            // Create byte buffer.
            byte[] bytes = new byte[1024];
            string published;
            try
            {
                publisher.Connect(endIP);
                Console.WriteLine("Connected to {0}", publisher.RemoteEndPoint.ToString());
                Console.ReadLine();
                // Encode the string into bytes.
                byte[] msg = Encoding.ASCII.GetBytes(data);
                Console.WriteLine(msg.ToString());

                // Send the bytes.
                int bytesSent = publisher.Send(msg);

                // Receive response from remote device.
                while (true)
                {
                    int bytesRec = publisher.Receive(bytes);
                    published = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    Console.WriteLine("Message received: {0}", published);
                    Console.ReadLine();
                }
                
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
