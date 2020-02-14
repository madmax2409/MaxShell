﻿using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.Generic;
namespace server
{
    class Program
    {
        public static string data = null;
        public static IPAddress ipAddress = null;
        public static Queue<string> machname = new Queue<string>();

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
            int bytesRec = s.Receive(bytes);
            data = Encoding.Unicode.GetString(bytes, 0, bytesRec);
            machname.Enqueue(data);
            Console.WriteLine("got a name " + data);
            while (true)
            {
                try
                {
                    bytesRec = s.Receive(bytes);
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
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
                if (ipHostInfo.AddressList[i].ToString().StartsWith("192"))
                    ipAddress = ipHostInfo.AddressList[i];

            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);  //FreeTcpPort()           
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(3);
            //Client handler = new Client()
            Console.WriteLine("listening");
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
    
   
    
    
