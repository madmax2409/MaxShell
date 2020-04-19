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
                msg = Encoding.Unicode.GetBytes(fullstring); //divides the data by the buffer size
                handler.Send(msg);  //and sends it in parts until the whole message is sent
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
                while (!flag) //checks if the password matches with the saved one
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
                bytesRec = s.Receive(bytes); //recieves data from the client to save
                data = Encoding.Unicode.GetString(bytes, 0, bytesRec);
                char[] sep = { '+' };
                string[] datas = data.Split(sep);
                Client.CheckAndAdd(s, datas[0], datas[1]);
                WmiFuncs.AddPaths(datas[0]);
                while (true) //and starts listenung to incoming requests
                {

                    bytesRec = s.Receive(bytes);
                    data = Encoding.Unicode.GetString(bytes, 0, bytesRec);
                    data = Server.CommandOutput(data, s);
                    SendPackets(data, s);
                    if (data.Contains("disconnect"))
                        break; // ends communication
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Client has disconnected");
            }
        }
    
        private static string SetPassword() //sets the login password that each client must input in order to login
        {
            Console.WriteLine("Choose a password, must be at least 4 chars length, and only alpahnumerical");
            Console.Write("Enter the password: ");
            string password = " ";
            while (!Regex.IsMatch(password, "^[a-zA-Z0-9]*$")) //the password must be alphanumerical
            {
                Console.Write("Enter the password: ");
                password = Console.ReadLine();
            }
            PreCon.SendApproval(); //once a password is set, all clients are informed
            Thread t = new Thread(new ThreadStart(PreCon.AfterStart));
            t.Start();
            return password;
        }

        
        static void Main(string[] args)
        {
            PreCon.FreeTcpPort(); //finds a free port
            PreCon.PreSock(); //tries to connect to each address
            Server.SetCommands(); //sets the server's commands
            WmiFuncs.AddPaths(Environment.MachineName); //saves the server's local shared folders
            byte[] bytes = new byte[4096];
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
                if (ipHostInfo.AddressList[i].ToString().StartsWith("192"))
                    ipAddress = ipHostInfo.AddressList[i];

            // builds the socket and starts listening for clients
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port); 
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            string pass  = SetPassword();
            Console.WriteLine("listening on ip: " + ipAddress + " port: " + port);
            listener.Listen(3); 
            while (true)
            {
                Socket handler = listener.Accept();
                Console.WriteLine("A client with the ip of " + handler.RemoteEndPoint + " has connected");
                //once a new client is connected, starts the socket maintenance function for him on a separate thread
                Thread t = new Thread(() => KeepInTact(handler, pass)); 
                t.Start();
            }
        }
    }
}
    
   
    
    
