﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PubSubLib
{
    
    public class Broker
    {
        private static IPInfo IP { get; set;}
        private static Dictionary<string, List<string>> SubInfo { get; set; } = new Dictionary<string, List<string>>();
        private static void SubThread()
        {
            byte[] bytes = new Byte[1024];
            IPEndPoint subEndIP = new(IP.Host, IP.SubPort);
            Socket subListener = new(IP.Host.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                subListener.Bind(subEndIP);
                subListener.Listen(10);

                // Start listening for connections.
                while (true)
                {
                    string data = null;
                    Console.WriteLine("Waiting for a connection...");
                    Console.WriteLine("Connected to " + subEndIP.ToString());
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = subListener.Accept();

                    // An incoming connection needs to be processed.  
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    Console.Write(data);
                    string[] lines = data.Split(" ");

                    if (String.Equals(lines[1], "sub"))
                    {
                        if (SubInfo.ContainsKey(lines[0]))
                        {
                            SubInfo[lines[0]].Add(lines[2]);
                        }
                        else
                        {
                            SubInfo.Add(lines[0], new List<string> { lines[2] });
                        }

                        // Show the data on the console.  
                        Console.WriteLine("ID: {0}, Topc: {1}", lines[0], lines[2]);

                        // Echo the data back to the client.  
                        byte[] msg = Encoding.ASCII.GetBytes("subbed to topic: " + lines[2]);

                        handler.Send(msg);

                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();

                    }
                    else if (String.Equals(lines[1], "unsub"))
                    {
                        SubInfo.Remove(lines[0]);
                        // Send unsub message.
                        Console.WriteLine("ID: {0} unsubbed from topic {1}", lines[0], lines[2]);
                        byte[] msg = Encoding.ASCII.GetBytes("unsub from " + lines[2]);

                        handler.Send(msg);

                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
        private static void ParseSubCommand()
        {
            // put the huge if statements in here.
        }

        private static void PubThread()
        {
            byte[] bytes = new Byte[1024];
            IPEndPoint pubEndIP = new(IP.Host, IP.PubPort);
            Socket publistener = new(IP.Host.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                publistener.Bind(pubEndIP);
                publistener.Listen(10);
                // Start listening for connections.
                while (true)
                {
                    string data = null;
                    Console.WriteLine("Waiting for a connection...");
                    Console.WriteLine(pubEndIP.ToString());
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = publistener.Accept();

                    // An incoming connection needs to be processed.  
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    Console.Write(data);
                    string[] lines = data.Split(
                        new string[] { "\n" }, StringSplitOptions.None);

                    // Show the data on the console.  
                    Console.WriteLine(lines[0] + ": " + lines[1]);

                    // Echo the data back to the client.  
                    byte[] msg = Encoding.ASCII.GetBytes("Received message: " + lines[1] + "(" + lines[0] + ")");
                    handler.Send(msg);

                    List<string> sublist = CheckSubs(lines[0]);

                    if (sublist.Count > 0)
                    {
                        IPEndPoint subEndIP = new(IP.Host, IP.SubRePort);
                        Socket subsender = new(IP.Host.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        subsender.Connect(subEndIP);
                        msg = Encoding.ASCII.GetBytes(lines[1] + "(" + lines[0] + ")");
                        subsender.Send(msg);
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
        private static List<string> CheckSubs(string topic)
        {
            var sublist = new List<string> { };

            foreach (string sub in SubInfo.Keys)
            {
                if (SubInfo[sub].Contains(topic))
                {
                    sublist.Add(sub);
                }
            }
            return sublist;
        }
        public static void StartBroker(IPInfo ip)
        {
            IP = ip;
            //Thread pub = new(PubThread);
            Thread sub = new(SubThread);
            //pub.Start();
            sub.Start();
        }
    }
}