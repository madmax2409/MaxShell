﻿using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace server
{
    class Program
    {
        public static string data = null;

        private static void SendPackets(string fullstring, Socket handler)
        {
            int counter = Encoding.ASCII.GetByteCount(fullstring);
            byte[] msg;
            while (counter >= 0)
            {
                msg = Encoding.Unicode.GetBytes(fullstring);
                handler.Send(msg);
                string dat = Encoding.Unicode.GetString(msg);
                fullstring = fullstring.Substring(dat.Length);
                counter -= 4096;
            }
        }
        private static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            File.WriteAllText(Environment.CurrentDirectory + "\\port.txt", port.ToString());
            return port;
        }
        private static void KeepInTact(Socket s)
        {
            byte[] bytes = new byte[4096];
            //Client handler = new Client();
            while (true)
            {
                try
                {
                    int bytesRec = s.Receive(bytes);
                    data = Encoding.Unicode.GetString(bytes, 0, bytesRec); 
                    Console.WriteLine("Got a message: " + data);
                    data = Server.CommandOutput(data);
                    SendPackets(data, s);
                    if (data.Contains("diconnect"))
                        break; // communication over
                }
                catch (SocketException)
                {
                    Console.WriteLine("Client has suddenly disconnectd");
                    break;
                }
            }
        }

        static void Main(string[] args)
        {
            byte[] bytes = new byte[4096];
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000 );  //FreeTcpPort()            
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(3);
<<<<<<< HEAD
            //Client handler = new Client()
            Console.WriteLine("listening");
            Socket handler = listener.Accept();
=======
>>>>>>> 00c2b2a37dc4f7a94ab8ec531def81cf03b2b28b
            Console.WriteLine("ready");
            while (true)
            {
                Socket handler = listener.Accept();
                Console.WriteLine("A client with the ip of " + handler.RemoteEndPoint + " has connected");
                Thread t = new Thread(() => KeepInTact(handler));
                t.Start();
            }
        }
    }
}
    
   
    
    
