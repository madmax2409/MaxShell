using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
        public static void SendApproval()
        {
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

        public static void PreSock()
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
                if (ipHostInfo.AddressList[i].ToString().StartsWith("192"))
                    ipAddress = ipHostInfo.AddressList[i];

            ProcessStartInfo psi = new ProcessStartInfo("cmd", "/c " + "arp -a");
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            Process pr = new Process();
            pr.StartInfo = psi;
            pr.Start();
            string output = pr.StandardOutput.ReadToEnd();
            op = output.Split(new char[] { '\n' });
            string[] ips;
            string port = FreeTcpPort().ToString();
            Program.port = int.Parse(port);
            foreach (string address in op)
                if (Regex.IsMatch(address, @"((?:\d{1,3}.){3}\d{1,3})\s+((?:[\da-f]{2}-){5}[\da-f]{2})\s+dynamic"))
                {
                    ips = address.Split(new char[] { ' ' });
                    Console.WriteLine(ips[2]);
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ips[2]), 11001);
                    Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        sender.Connect(remoteEP);
                        byte[] msg = Encoding.Unicode.GetBytes(ipAddress + " " +  port);
                        sender.Send(msg);
                        connected.Enqueue(remoteEP);
                    }
                    catch { }
                }
        }
    }
}
