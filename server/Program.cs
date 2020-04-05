using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;


namespace server
{
    class Program
    {
        public static string data = null;
        public static IPAddress ipAddress = null;
        public static int port;

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

        private static void KeepInTact(Socket s, string pass)
        {
            byte[] bytes = new byte[4096];
            bool flag = false;
            int bytesRec;
            try
            {
                while (!flag)
                {
                    bytesRec = s.Receive(bytes);
                    data = Encoding.Unicode.GetString(bytes, 0, bytesRec);
                    if (data == pass)
                    {
                        SendPackets("good to go", s);
                        flag = true;
                    }
                    else
                        SendPackets("wrong password", s);
                }
                bytesRec = s.Receive(bytes);
                data = Encoding.Unicode.GetString(bytes, 0, bytesRec);
                char[] sep = { '+' };
                string[] datas = data.Split(sep);
                Client.CheckAndAdd(s, datas[0], datas[1]);
                WmiFuncs.AddPaths(datas[0]);
                while (true)
                {

                    bytesRec = s.Receive(bytes);
                    data = Encoding.Unicode.GetString(bytes, 0, bytesRec);
                    Console.WriteLine("Got a message: " + data);
                    data = Server.CommandOutput(data, s);
                    SendPackets(data, s);
                    if (data.Contains("disconnect"))
                        break; // communication over
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Client has suddenly disconnected");
            }
        }
    
        public static string SetPassword()
        {
            Console.WriteLine("Choose a password, must be at least 4 chars length, and only alpahnumerical");
            Console.Write("Enter the password: ");
            string password = " ";
            while (!Regex.IsMatch(password, "^[a-zA-Z0-9]*$"))
            {
                Console.Write("Enter the password: ");
                password = Console.ReadLine();
            }
            PreCon.SendApproval();
            return password;
        }

        
        static void Main(string[] args)
        {
            PreCon.PreSock();
            Server.SetCommands();
            WmiFuncs.AddPaths(Environment.MachineName);
            byte[] bytes = new byte[4096];
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
                if (ipHostInfo.AddressList[i].ToString().StartsWith("192"))
                    ipAddress = ipHostInfo.AddressList[i];

            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);  //FreeTcpPort()           
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            string pass  = SetPassword();
            Console.WriteLine("listening");
            listener.Listen(3);
            while (true)
            {
                Socket handler = listener.Accept();
                Console.WriteLine("A client with the ip of " + handler.RemoteEndPoint + " has connected");
                Thread t = new Thread(() => KeepInTact(handler, pass));
                t.Start();
            }
        }
    }
}
    
   
    
    
