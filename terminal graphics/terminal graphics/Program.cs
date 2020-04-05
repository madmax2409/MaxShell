using System;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace terminal_graphics
{
    static class Program
    {
        private static Socket sender;
        private static Socket sender2;
        private static byte[] bytes = new byte[4096];
        public static Form2 form2;
        public static bool check = false;
        public static Socket s;
        public static int port;
        public static IPAddress ip;

        private static void Connection()
        {
            IPEndPoint remoteEP = new IPEndPoint(ip, port); //IPAddress.Parse(ip), port
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s = sender;
            sender.Connect(remoteEP);
            Login_Window form3 = new Login_Window(s);
            form3.ShowDialog();
            while (!check)
            {
                Thread.Sleep(1000);
            }
            
            sender.Send(Encoding.Unicode.GetBytes(Environment.MachineName + "+" + Login_Window.nickname));
            if (!Directory.Exists("C:\\dump_folders"))
                CallFunc("sharefolder on " + Environment.MachineName + " where dir='C:\\dump_folders'");
        }

        public static string Maintain(string message)
        {
            message = message.ToLower();
            try
            {
                if (message == "file manager")
                {
                    string data = CallFunc("showfolders");
                    data = Form2.AddInsides(data);
                    form2 = new Form2(data);
                    form2.Show();
                    return "opened file manager";
                }
                else if (message == "disconnect")
                {
                    Exit_WIndow ew = new Exit_WIndow();
                    ew.ShowDialog();
                    return "";
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
            catch (SocketException)
            {
                MessageBox.Show("The Server is down, closing...", "Sorry!");
                Application.Exit();
                return "";
            }
        }

        public static string CallFunc(string command)
        {
            sender.Send(Encoding.Unicode.GetBytes(command));
            int bytesRec = sender.Receive(bytes);
            return Encoding.Unicode.GetString(bytes, 0, bytesRec);
        }

        static void Main()
        {
            GetAddr.PreSock();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Connection();
            Application.Run(new Shell());
        }
    }
}
