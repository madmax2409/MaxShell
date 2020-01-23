using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace server
{
    class Program
    {
        public static string data = null;
        public static byte[] bytes = new byte[4096];
        public static IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
        public static IPAddress ipAddress = ipHostInfo.AddressList[0];
        public static IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
        public static Socket listener;
        public static void Maintenance(Socket cl)
        {
            Client handler = new Client(cl);
            handler.SetSocket(cl);
            while (true)
            {
                int bytesRec = cl.Receive(bytes);
                data = Encoding.ASCII.GetString(bytes, 0, bytesRec); // encode by len
                data = Server.CommandOutput(data);
                bytes = Encoding.ASCII.GetBytes(data);
                cl.Send(bytes);
                if (data.Contains("disconnect"))
                {
                    Client.Disconnect(cl);
                    Console.WriteLine(cl.RemoteEndPoint.ToString() + " disconnected");
                    break; // communication over. Now send back and close socket
                }
            }
            byte[] msg = Encoding.ASCII.GetBytes(data);
            cl.Send(msg);
            cl.Shutdown(SocketShutdown.Both);
            cl.Close();
        }

        static void Main(string[] args)
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint); 
            listener.Listen(5);
            while (true)
            {
                Socket cl = listener.Accept();
                Console.WriteLine(cl.RemoteEndPoint.ToString() + " just connected");
                Thread t = new Thread(() => Maintenance(cl));
                t.Start();
            }
        }
    }
}
    
   
    
    
