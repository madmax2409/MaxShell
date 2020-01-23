using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace server
{
    class Program
    {
        public static string data = null;
        
        static void Main(string[] args)
        {
            byte[] bytes;
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName()); 
            IPAddress ipAddress = ipHostInfo.AddressList[0]; 
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint); 
            listener.Listen(3);
            //Client handler = new Client()
            Socket handler = listener.Accept();
            
            while (true)
            {
                bytes = new byte[4096];
                int bytesRec = handler.Receive(bytes);
                Console.WriteLine("Got a message");
                data = Encoding.ASCII.GetString(bytes, 0, bytesRec); // encode by len
                data = Server.CommandOutput(data);
                bytes = Encoding.ASCII.GetBytes(data);
                handler.Send(bytes);
                if (data.Contains("diconnect")) 
                  break; // communication over. Now send back and close socket
            }
            byte[] msg = Encoding.ASCII.GetBytes(data); 
            handler.Send(msg); 
            handler.Shutdown(SocketShutdown.Both); 
            handler.Close();
        }
    }
}
    
   
    
    
