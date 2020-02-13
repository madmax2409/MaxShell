using System;
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

        public static void SendPackets(string fullstring, Socket handler)
        {
            int counter = Encoding.ASCII.GetByteCount(fullstring);
            byte[] msg;
            Console.WriteLine(fullstring);
            while (counter >= 0)
            {
                msg = Encoding.Unicode.GetBytes(fullstring);
                handler.Send(msg);
                string dat = Encoding.Unicode.GetString(msg);
                fullstring = fullstring.Substring(dat.Length);
                counter -= 4096;
            }
        }
        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            File.WriteAllText(Environment.CurrentDirectory + "\\port.txt", port.ToString());
            return port;
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
            //Client handler = new Client()
            Console.WriteLine("listening");
            Socket handler = listener.Accept();
            Console.WriteLine("ready");
            while (true)
            {
                int bytesRec = handler.Receive(bytes);
                data = Encoding.Unicode.GetString(bytes, 0, bytesRec); // encode by len
                Console.WriteLine("Got a message: " + data);
                data = Server.CommandOutput(data);
                SendPackets(data, handler);
                if (data.Contains("diconnect"))
                    break; // communication over. Now send back and close socket
            }
            byte[] msg = Encoding.Unicode.GetBytes(data);
            handler.Send(msg);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }
}
    
   
    
    
