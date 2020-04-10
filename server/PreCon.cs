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

        public static void FreeTcpPort()
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

        private static void TryCon(Socket sender, IPEndPoint remoteEP)
        {
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
                if (ipHostInfo.AddressList[i].ToString().StartsWith("192"))
                    ipAddress = ipHostInfo.AddressList[i];

            Stopwatch sw = new Stopwatch();
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
            foreach (string address in op)
                if (Regex.IsMatch(address, @"((?:\d{1,3}.){3}\d{1,3})\s+((?:[\da-f]{2}-){5}[\da-f]{2})\s+dynamic"))
                {
                    ips = address.Split(new char[] { ' ' });
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ips[2]), 11001);
                    Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    t = new Thread(() => TryCon(sender, remoteEP));
                    try
                    {
                        t.Start();
                        Thread.Sleep(1000);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            
        /*
















































            private void ClientConnectCallback(IAsyncResult ar)
            {
                try
                {
                    // Retrieve the socket from the state object.  
                    Socket client = (Socket)ar.AsyncState;

                    // Complete the connection.  
                    client.EndConnect(ar);

                    // Disable the Nagle Algorithm for this tcp socket.
                    client.NoDelay = true;
                    // Set the receive buffer size to 4k
                    client.ReceiveBufferSize = 4096;
                    // Set the send buffer size to 4k.
                    client.SendBufferSize = 4096;

                    isConnected = true;
                }
                catch (Exception e)
                {
                    UnityThread.executeInUpdate(() =>
                    {
                        networkErrorMessage.SetActive(true);
                    });

                    Debug.Log("Something went wrong and the socket couldn't connect");
                    Debug.Log(e.ToString());
                    return;
                }

                // Setup done, ConnectDone.
                Debug.Log(string.Format("Socket connected to {0}", clientSock.RemoteEndPoint.ToString()));
                Debug.Log("Connected, Setup Done");

                // Start the receive thread
                StartReceive();
                new Thread ()
            }

            private void InitializeNetworking()
            {
                // Establish the remote endpoint for the socket.  
                IPAddress ipAddress = ClientInfo.ipAddress;
                IPEndPoint remoteEndPoint = ClientInfo.remoteEP;
                // Create a TCP/IP socket.  
                clientSock = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // Connect to the remote endpoint.  
                    clientSock.BeginConnect(remoteEndPoint,
                        new AsyncCallback(ClientConnectCallback), clientSock);
                }
                catch (Exception e)
                {
                    Debug.Log("Something went wrong and the socket couldn't connect");
                    Debug.Log(e);
                }
            }


















    */


        }
    }
}

