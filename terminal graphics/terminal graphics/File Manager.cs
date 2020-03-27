using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace terminal_graphics
{
    public partial class Form2 : Form
    {
        private static Button open = new Button();
        private static Button refresh = new Button();
        private static TreeView tv = new TreeView();
        private static string filechoice = "";
        private static TreeNode temp;
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
            catch (NullReferenceException)
            {
                temp = e.Node;
                filechoice = e.Node.Text;
            }
        }
        public static string AddInsides(string data)
        {
            string temp, mach = "";
            string[] datas = data.Split(new char[] { '\n' });
            for(int i = 0; i<datas.Length;i++)
            {
                if (datas[i].IndexOf(Environment.MachineName) == -1 && datas[i].IndexOf("'s shared folders and drives") != -1)
                    mach = datas[i].Substring(0, datas[i].IndexOf("'s shared folders and drives"));

                else if (mach != "" && mach != Environment.MachineName)
                {
                    temp = Program.CallFunc("listfiles on " + mach + " where dir='" + datas[i] + "'");
                    if (temp.IndexOf("\\") != -1)
                        data = data.Insert(data.IndexOf(datas[i]) + datas[i].Length, "\n" + temp.Substring(0, temp.IndexOf("stoprightnow")));
                }
            }
            return data;
        }
        private static void OpenFile(object sender, EventArgs e)
        {
            if (dirchoice != "")
                if (dirchoice == "My shared folders" || dirchoice == "My Dump Folders")
                {
                    if (filechoice != "" && filechoice.IndexOf(Environment.MachineName) == -1 && filechoice != "My Dump Folders") //"My shared Folders!"
                        Process.Start(filechoice);
                }
                else if (temp != null && temp.Nodes == null)
                {
                    int end = dirchoice.IndexOf("'s shared folders and drives");
                    string st = filechoice.Substring(filechoice.IndexOf("$")-1).Replace("$", ":");
                    Program.CallFunc("copyfile from " + dirchoice.Substring(0, end) + " where dir='" + filechoice + "'");
                }
                    
        }

        private static void Refresh(object sender, EventArgs e)
        {
            string dirs = Program.CallFunc("showfolders");
            dirs = AddInsides(dirs);
            string[] direcs = dirs.Split(new char[] { '\n' });
            tv.Nodes.Clear();
            BuildTree(direcs);
        }
        private static void BuildTree(string[] direcs)
        {
            TreeNode tn = new TreeNode();
            TreeNode temp = null;
            tn.Name = "sourcenode";
            tn.Text = "My shared folders";
            tv.Nodes.Add(tn);
            TreeNode temp2 = tn;
            foreach (string dir in direcs)
                if (dir.Length > 3 && dir != "stoprightnow" && dir != "C:\\dump_folders")
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
                        if (dir[0] == '\\')
                            temp.Nodes.Add(dir);
                        else
                        {
                            temp = temp2.Nodes.Add(dir);
                            ProcessDirectory(dir, temp);
                        }
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
            string[] direcs = dirs.Split(new char[] { '\n' });
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