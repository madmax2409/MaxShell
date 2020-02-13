using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace terminal_graphics
{
    public partial class Form2 : Form
    {
        private static Socket sender;
        private static Button b = new Button();
        private static TreeView tv = new TreeView();
        private static string choice;
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
        private static void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            choice = e.Node.Text;
        }

        private static void OpenFile(object sender, EventArgs e)
        {
            Process.Start(choice);
        }
        public Form2(string dirs)
        {

            char[] seperator = { '\n' };
            string[] direcs = dirs.Split(seperator);

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
            tv.Location = new Point(0, 0);
            tv.Size = new Size(400, 400);
            tv.BorderStyle = BorderStyle.FixedSingle;
            tv.AfterSelect += new TreeViewEventHandler(treeView1_AfterSelect);
            Controls.Add(tv);

            b.Font = new Font("comic sans", 10);
            b.Location = new Point(20, 410);
            b.Text = "open";
            b.Click += new EventHandler(OpenFile);
            b.Size = new Size(70, 30);
            Controls.Add(b);

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            InitializeComponent();
        }
    }
}