using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace server
{
    class PreCon
    {
        public static IPAddress ipAddress;
        public static string[] op;
        public static Queue<IPEndPoint> connected = new Queue<IPEndPoint>();
        private static Thread t;

        public static void FreeTcpPort() //gets the next free tcp port to be the one which ther server will use
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            string temport = port.ToString();
            Program.port = int.Parse(temport);
        }

        public static void AfterStart() 
        {
            while (true)
            {
                PreSock();
                SendApproval();
            }

        }

        public static void SendApproval()
        {   //after inital connection was made, the approval of login is sent to all connected clients
            while (connected.Count > 0)
            {
                IPEndPoint remoteEP = connected.Dequeue();
                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    sender.Connect(remoteEP);
                    byte[] msg = Encoding.Unicode.GetBytes("approved");
                    sender.Send(msg);
                }
                catch { }
            }  
        }

        private static void TryCon(Socket sender, IPEndPoint remoteEP)
        {   //tries to connect and send the address of connection to the client
            try
            {
                sender.Connect(remoteEP);
                byte[] msg = Encoding.Unicode.GetBytes(ipAddress + " " + Program.port);
                sender.Send(msg);
                connected.Enqueue(remoteEP);
            }
            catch { }
        }
        public static void PreSock()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList[0];
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
                if (ipHostInfo.AddressList[i].ToString().StartsWith("192")) //gets the local ip
                    ipAddress = ipHostInfo.AddressList[i];

            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11001);
            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            t = new Thread(() => TryCon(sender, remoteEP)); //tries to connect to each ip and save if succeeded
            try
            {
                t.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Stopwatch sw = new Stopwatch();
            ProcessStartInfo psi = new ProcessStartInfo("cmd", "/c " + "arp -a") //gets the list of all avalible ip's 
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process pr = new Process { StartInfo = psi };
            pr.Start();
            string output = pr.StandardOutput.ReadToEnd();
            op = output.Split(new char[] { '\n' });
            string[] ips;
            foreach (string address in op)
                if (Regex.IsMatch(address, @"((?:\d{1,3}.){3}\d{1,3})\s+((?:[\da-f]{2}-){5}[\da-f]{2})\s+dynamic"))
                {
                    ips = address.Split(new char[] { ' ' });
                    remoteEP = new IPEndPoint(IPAddress.Parse(ips[2]), 11001);
                    sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    t = new Thread(() => TryCon(sender, remoteEP)); //tries to connect to each ip and save if succeeded
                    try
                    {
                        t.Start();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
        }
    }
}

