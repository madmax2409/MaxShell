using System;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace terminal_graphics
{
    static class Program
    {
        public static IPAddress ip;
        private static Socket sender;
        public static Socket s;
        public static int port = 11000;
        private static readonly byte[] bytes = new byte[4096];
        public static bool check = false;
        public static Form2 form2;

        private static void Connection() //recieve the address of the server and begin connection
        {
            IPEndPoint remoteEP = new IPEndPoint(ip, port); //ip, port
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s = sender;
            sender.Connect(remoteEP);
            Login_Window form3 = new Login_Window(s);
            form3.ShowDialog();
            while (!check)
            {
                Thread.Sleep(1000);
            }
            
            sender.Send(Encoding.Unicode.GetBytes(Environment.MachineName + "+" + Login_Window.nickname)); //server needs the info to operate and execute functions
            sender.Send(Encoding.Unicode.GetBytes("about"));
            int bytesRec = sender.Receive(bytes);
            Shell.firstdata = Encoding.Unicode.GetString(bytes, 0, bytesRec); //pass the "about" text of the start of the project
            if (!Directory.Exists("C:\\MaxShell\\dump_folders")) //if we already have a dump, we share to enable the reading of it by the file manager
                CallFunc("sharefolder on " + Environment.MachineName + " where dir='C:\\MaxShell\\dump_folders'");
        }

        public static string Maintain(string message, string mach = "")
        {
            message = RemoveSpaces(message); //remove extras spaces from the command
            try
            {
                if (message == "file manager") //check for keywords that represnt special commands
                {
                    string data = CallFunc("shared folders on " + Environment.MachineName); //recieve the data to build the treeview 
                    data = Form2.AddInsides(data);
                    form2 = new Form2(data); //and start the building
                    form2.Show();
                    return "opened file manager";
                }
                else if (message == "disconnect")
                {
                    Exit_WIndow ew = new Exit_WIndow();
                    ew.ShowDialog();
                    return "";
                }
                else if (message == "create"  || message == "delete" || message == "run" || message == "kill") 
                {
                    Create c = new Create(CallFunc("clients"), message, mach);
                    c.ShowDialog();
                    return "Opened " + message.Substring(0, message.Length - 1) + "ion window";
                }
                else
                {
                    message = message.Replace("My Computer", Environment.MachineName); 

                    byte[] msg = Encoding.Unicode.GetBytes(message);
                    sender.Send(msg);
                    string totaldata = "", data = "";
                    do
                    {
                        //if a message is longer than 4096 bytes, we rebuild it till the stop sign and then send for display
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
                    return totaldata;
                }
            }
            catch (SocketException)
            {
                MessageBox.Show("The Server is down, closing...", "Sorry!"); //if the socket is destroyet, it's safe to assume the server is down
                Application.Exit();
                return "";
            }
        }

        private static string RemoveSpaces(string str)
        {
            int spacecount = 0;
            for (int i = 0; i < str.Length; i++) // erase spaces before and after the command
            {
                if (str[i] == ' ')
                    spacecount++;
                else
                    break;
            }
            str = str.Remove(0, spacecount);
            spacecount = 0;
            for (int i = str.Length-1 ; i >= 0; i++)
            {
                if (str[i] == ' ')
                    spacecount++;
                else
                    break;
            }
            str = str.Remove(str.Length-spacecount, spacecount);
            return str;
        }

        public static string CallFunc(string command)
        {
            sender.Send(Encoding.Unicode.GetBytes(command));
            int bytesRec = sender.Receive(bytes);
            return Encoding.Unicode.GetString(bytes, 0, bytesRec);
        }

        public static void EmptyDumps()
        {
            string[] dirs = Directory.GetDirectories(@"C:\MaxShell\dump_folders");
            foreach (string dir in dirs)
                Directory.Delete(dir, true);
        }

        [STAThread]
        static void Main()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
            try { EmptyDumps(); } catch { };
            GetAddr.PreSock();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Connection();
            Application.Run(new Shell());
        }
    }
}
