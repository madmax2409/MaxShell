using System;
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
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
                if (ipHostInfo.AddressList[i].ToString().StartsWith("172"))
                    ipAddress = ipHostInfo.AddressList[i];

            //ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("192.168.1.249"), 11000); //int.Parse(File.ReadAllText(GetTheRightPath()))
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(remoteEP);
            sender.Send(Encoding.Unicode.GetBytes(Environment.MachineName + "+" + Login_Window.nickname));

            if (!Directory.Exists("C:\\dump_folders"))
                CallFunc("sharefolder on " + Environment.MachineName + " where dir='C:\\dump_folders'");
        }
        public static string Maintain(string message)
        {
            try
            {
                if (message == "file manager")
                {
                    string data = CallFunc("showfolders");
                    data = Form2.AddInsides(data);
                    form2 = new Form2(data);
                    Thread t = new Thread(() => Application.Run(form2));
                    t.Start();
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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Login_Window form3 = new Login_Window();
            form3.ShowDialog();
            Thread con = new Thread(new ThreadStart(Connection));
            con.Start();
            Application.Run(new Shell());
        }
    }
}
