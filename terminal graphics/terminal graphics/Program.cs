using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace terminal_graphics
{
    static class Program
    {
        private static Socket sender;
        private static byte[] bytes = new byte[4096];
        private static IPAddress ipAddress;
        public static Form2 form2;
        private static void Connection()
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, int.Parse(File.ReadAllText(GetTheRightPath())));  
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(remoteEP);
        }
        public static string Maintain(string message)
        {
            if (message == "file manager")
            {
                
                sender.Send(Encoding.Unicode.GetBytes("showfolder"));
                int bytesRec = sender.Receive(bytes);
                string data = Encoding.Unicode.GetString(bytes, 0, bytesRec);
                form2 = new Form2(data);
                Thread t = new Thread(() => Application.Run(form2));
                t.Start();
                return "opened file manager";
            }
            else
            {
                byte[] msg = Encoding.Unicode.GetBytes(message);
                sender.Send(msg);
                string totaldata = "", data = "";
                do
                {
                    int bytesRec = sender.Receive(bytes);
                    data = Encoding.Unicode.GetString(bytes, 0, bytesRec);
                    totaldata += data;
                }
                while (!data.Contains("stoprightnow"));
                if (totaldata == "diconnect")
                {
                    sender.Shutdown(SocketShutdown.Both); 
                    sender.Close(); // destroy the socket
                }
                return totaldata.Remove(totaldata.IndexOf("stoprightnow"), 12);
            }
        }
        private static string GetTheRightPath()
        {
            string curpath = Environment.CurrentDirectory;
            char[] separator = { '\\' };
            string[] dirs = curpath.Split(separator);
            string output = "";
            for (int i = 0; i < dirs.Length; i++)
                if (i < dirs.Length - 4)
                    output += dirs[i] + "\\";
            return output + "\\server\\bin\\Debug\\port.txt";
            
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
