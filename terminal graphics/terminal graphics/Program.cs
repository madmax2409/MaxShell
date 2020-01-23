using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace terminal_graphics
{
    static class Program
    {
        private static Socket sender;
        private static byte[] bytes = new byte[4096];
        public static void Connection()
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(remoteEP);
        }
        public static string Maintain(string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message);
            sender.Send(msg);
            int bytesRec = sender.Receive(bytes);
            string data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            if (data == "diconnect")
            {
                sender.Shutdown(SocketShutdown.Both); // “send”, “receive”, “both
                sender.Close(); // destroy the socket.
            }
            return data;
        }
        static void Main()
        {
            Thread con = new Thread(new ThreadStart(Connection));
            con.Start();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
