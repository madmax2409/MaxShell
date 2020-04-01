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
        private Button open = new Button();
        private Button refresh = new Button();
        private Button delete = new Button();
        private Button create = new Button();
        private static TreeView tv = new TreeView();
        private static string filechoice = "";
        private static TreeNode tamp;
        private static string dirchoice = "";
        private static TreeNode dumps;
        private static Queue<bool> q = new Queue<bool>();

        public static void CopyTreeNodes(TreeView treeview1, Queue<bool> q)
        {
            foreach (TreeNode tn in treeview1.Nodes)
            {
                q.Enqueue(tn.IsExpanded);
                CopyChildren(tn);
            }
        }
        public static void CopyChildren(TreeNode original)
        {
            foreach (TreeNode tn in original.Nodes)
            {
                q.Enqueue(tn.IsExpanded);
                CopyChildren(tn);
            }
        }

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
                    bool val = q.Dequeue();
                    if (val)
                        tempNode.Expand();
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
                tamp = e.Node;
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
                else if (filechoice != null && !Directory.Exists(filechoice))
                {
                    int end = dirchoice.IndexOf("'s shared folders and drives");
                    Program.CallFunc("copyfile from " + dirchoice.Substring(0, end) + " where dir='" + filechoice + "'");
                    RefreshWindow();
                    Process.Start(dumps.LastNode.LastNode.Text);
                }
                  
        }
        public static void Refresh(object sender, EventArgs e)
        {
            CopyTreeNodes(tv, q);
            RefreshWindow();

        }
        public static void RefreshWindow()
        {
            string dirs = Program.CallFunc("showfolders");
            dirs = AddInsides(dirs);
            string[] direcs = dirs.Split(new char[] { '\n' });
            tv.Nodes.Clear();
            BuildTree(direcs);
        }
        public static void DeleteFile(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to delete the file?", "Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                if (!Directory.Exists(filechoice))
                {
                    if (dirchoice == "My shared folders")
                        File.Delete(tamp.Text);

                    else if (dirchoice == "My Dump Folders")
                    {
                        File.Delete(tamp.Text);
                        Directory.Delete(tamp.Parent.Text);
                    }
                    else if (tamp != null && !Directory.Exists(tamp.Text))
                    {
                        int end = dirchoice.IndexOf("'s shared folders and drives");
                        Program.CallFunc("deletefile from " + dirchoice.Substring(0, end) + " where dir='" + filechoice + "'");
                    }
                }
                else
                {
                    if (dirchoice == "My shared folders")
                    {
                        Directory.Delete(filechoice, true);
                    }

                    else if (dirchoice == "My Dump Folders")
                    {
                        File.Delete(tamp.LastNode.Text);
                        Directory.Delete(tamp.Text);
                    }
                    else if (tamp != null && !Directory.Exists(tamp.Text))
                    {
                        int end = dirchoice.IndexOf("'s shared folders and drives");
                        Program.CallFunc("deletefile from " + dirchoice.Substring(0, end) + " where dir='" + filechoice + "'");
                    }
                }
                RefreshWindow();
            }
        }

        public static void CreateFile(object sender, EventArgs e)
        {
            Create cr = new Create();
            cr.ShowDialog();
        }

        private static void BuildTree(string[] direcs)
        {
            TreeNode tn = new TreeNode();
            TreeNode temp = null;
            tn.Name = "sourcenode";
            tn.Text = "My shared folders";
            tv.Nodes.Add(tn);
            TreeNode temp2 = tn;
            bool val = false;
            foreach (string dir in direcs)
                if (dir.Length > 3 && dir != "stoprightnow" && dir != "C:\\dump_folders")
                {
                    if (q.Count != 0)
                        val = q.Dequeue();

                    if (dir.Contains("'s shared folders and drives"))
                        if (dir.Substring(0, dir.IndexOf("'s shared folders and drives")) != Environment.MachineName)
                        {
                            temp2 = new TreeNode();
                            temp2.Name = dir;
                            temp2.Text = dir;
                            tv.Nodes.Add(temp2);
                            if (val)
                                temp2.Expand();
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
                            if (val)
                                temp.Expand();
                            ProcessDirectory(dir, temp);
                        }
                    }
                }
            ShowDumps();
        }

        public static void ShowDumps()
        {
            if (Directory.Exists("C:\\dump_folders"))
            {
                dumps = new TreeNode("My Dump Folders");
                tv.Nodes.Add(dumps);
                foreach (string dir in Directory.GetFileSystemEntries("C:\\dump_folders"))
                {
                    TreeNode temp = new TreeNode(dir);
                    dumps.Nodes.Add(temp);
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
            open.BackColor = Color.Yellow;
            Controls.Add(open);

            refresh.Font = new Font("comic sans", 10);
            refresh.Location = new Point(100, 410);
            refresh.Text = "refresh";
            refresh.Click += delegate (object sender, EventArgs e) { Refresh(sender, e); };
            refresh.Size = new Size(70, 30);
            refresh.BackColor = Color.Turquoise;
            Controls.Add(refresh);

            delete.Font = new Font("comic sans", 10);
            delete.Location = new Point(180, 410);
            delete.Text = "delete";
            delete.Click += new EventHandler(DeleteFile);
            delete.Size = new Size(70, 30);
            delete.BackColor = Color.Tomato;
            Controls.Add(delete);

            create.Font = new Font("comic sans", 10);
            create.Location = new Point(260, 410);
            create.Text = "create";
            create.Click += new EventHandler(CreateFile);
            create.Size = new Size(70, 30);
            create.BackColor = Color.SpringGreen;
            Controls.Add(create);

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            InitializeComponent();
        }
    }
}