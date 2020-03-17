using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace terminal_graphics
{
    public partial class Form2 : Form
    {
        private static Button open = new Button();
        private static Button refresh = new Button();
        private static TreeView tv = new TreeView();
        private static string filechoice = "";
        private static string dirchoice = "";
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
            TreeNode temp = e.Node;
            try
            {
                while (true)
                {
                    temp = temp.Parent;
                    dirchoice = temp.Text;
                }
            }
            catch
            {
                filechoice = e.Node.Text;
            }
        }

        private static void OpenFile(object sender, EventArgs e)
        {
            if (dirchoice != "")
                if (dirchoice == "My Shared Folders" || dirchoice == "My Dump Folders")
                {
                    if (filechoice != "" && filechoice != "My Shared Folders" && filechoice != "My Dump Folders")
                        Process.Start(filechoice);
                }
                else
                {
                    int end = dirchoice.IndexOf("'s shared folders and drives");
                    Program.CallFunc("CopyFile " + dirchoice.Substring(0, end - 1) + " " + filechoice);
                }
                    
        }

        private static void Refresh(object sender, EventArgs e)
        {
            string dirs = Program.CallFunc("showfolders");
            char[] seperator = { '\n' };
            string[] direcs = dirs.Split(seperator);
            tv.Nodes.Clear();
            BuildTree(direcs);
        }
        private static void BuildTree(string[] direcs)
        {
            TreeNode tn = new TreeNode();
            TreeNode temp;
            tn.Name = "sourcenode";
            tn.Text = "My Shared Folders";
            tv.Nodes.Add(tn);
            TreeNode temp2 = tn;
            foreach (string dir in direcs)
                if (dir.Length < 3 || dir != "stoprightnow" || dir.Contains("$"))
                    if (dir.Contains("'s shared folders and drives"))
                        if (dir.Substring(0, dir.IndexOf("'s shared folders and drives")) != Environment.MachineName)
                        {
                            temp2 = new TreeNode();
                            temp2.Name = dir;
                            temp2.Text = dir;
                            tv.Nodes.Add(temp2);
                        }
                        else
                            continue;
                    else
                    {
                        temp = temp2.Nodes.Add(dir);
                        ProcessDirectory(dir, temp);
                    }
            ShowDumps();
        }

        public static void ShowDumps()
        {
            if (Directory.Exists("C:\\dump_folders"))
            {
                TreeNode tn = new TreeNode("My Dump Folders");
                tv.Nodes.Add(tn);
                foreach (string dir in Directory.GetFileSystemEntries("C:\\dump_folders"))
                {
                    TreeNode temp = new TreeNode(dir);
                    tn.Nodes.Add(temp);
                    ProcessDirectory(dir, temp);
                }
            }
            else
                Directory.CreateDirectory("C:\\dump_folders");
        }
        public Form2(string dirs)
        {
            char[] seperator = { '\n' };
            string[] direcs = dirs.Split(seperator);
            BuildTree(direcs);
            
            tv.Font = new Font("comic sans", 10);
            tv.Location = new Point(0, 0);
            tv.Size = new Size(400, 400);
            tv.BorderStyle = BorderStyle.FixedSingle;
            tv.AfterSelect += new TreeViewEventHandler(treeView1_AfterSelect);
            Controls.Add(tv);

            open.Font = new Font("comic sans", 10);
            open.Location = new Point(20, 410);
            open.Text = "open";
            open.Click += new EventHandler(OpenFile);
            open.Size = new Size(70, 30);
            Controls.Add(open);

            refresh.Font = new Font("comic sans", 10);
            refresh.Location = new Point(100, 410);
            refresh.Text = "refresh";
            refresh.Click += delegate (object sender, EventArgs e) { Refresh(sender, e); };
            refresh.Size = new Size(70, 30);
            Controls.Add(refresh);

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            InitializeComponent();
        }
    }
}