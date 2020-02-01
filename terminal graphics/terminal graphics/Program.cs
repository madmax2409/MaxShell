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
            if (message == "file manager")
            {
                Application.Run(new Form2());
                return "opened file manager";
            }
            else
            {
                byte[] msg = Encoding.ASCII.GetBytes(message);
                sender.Send(msg);
                string totaldata = "", data = "";
                do
                {
                    MessageBox.Show("abraham");
                    int bytesRec = sender.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    totaldata += data;
                }
                while (!data.Contains("stoptightnow"));
                if (totaldata == "diconnect")
                {
                    sender.Shutdown(SocketShutdown.Both); // “send”, “receive”, “both
                    sender.Close(); // destroy the socket.
                }
                return totaldata;
            }
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
