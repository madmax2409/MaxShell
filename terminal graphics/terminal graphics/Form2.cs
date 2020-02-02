using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace terminal_graphics
{
    public partial class Form2 : Form
    {
        private static Socket sender;
        private static void ProcessDirectory(string dir, TreeNode Node)
        {
            try
            {
                string[] SubDir;
                SubDir = Directory.GetFileSystemEntries(dir);
                foreach (string SB in SubDir) // exit upon empty 
                {
                    TreeNode tempNode = new TreeNode();
                    tempNode.Text = SB;
                    Node.Nodes.Add(tempNode);
                    ProcessDirectory(SB, tempNode); // recursive call per node
                }
            }
            catch
            {

            }
        }
        private static void Connection()
        {

            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000 );    //int.Parse(File.ReadAllText(Program.GetTheRightPath()))
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(remoteEP);
        }
        public Form2(string dirs)
        {
            
            char[] seperator = { '\n' };
            string[] direcs = dirs.Split(seperator);

            TreeView tv = new TreeView();
            TreeNode tn = new TreeNode();
            TreeNode temp;
            tn.Name = "sourcenode";
            tn.Text = "shared folders";
            tv.Nodes.Add(tn);
            foreach (string dir in direcs)
                if (dir != "stoprightnow")
                {
                    temp = tn.Nodes.Add(dir);
                    ProcessDirectory(dir, temp);
                }
            tv.Font = new Font("comic sans", 10);
            tv.Location = new Point(0,0);
            tv.Size = new Size(400, 450);
            tv.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(tv);
        
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            InitializeComponent();
        }
    }
}
