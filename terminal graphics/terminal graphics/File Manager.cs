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
        private static TreeView tv;
        public static string filechoice = "";
        public static string dirchoice = "";
        private static TreeNode tamp;
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
                SubDir = Directory.GetFiles(dir);
                foreach (string SB in SubDir) // exit upon empty 
                {
                    TreeNode tempNode = new TreeNode();
                    tempNode.Text = SB;
                    Node.Nodes.Add(tempNode);
                    //bool val = q.Dequeue();
                    //if (val)
                        //tempNode.Expand();
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
                    temp = Program.CallFunc("list " + datas[i] + " on " + mach);
                    if (temp.IndexOf("\\") != -1)
                        data = data.Insert(data.IndexOf(datas[i]) + datas[i].Length, "\n" + temp.Substring(0, temp.IndexOf("stoprightnow")));
                }
            }
            return data;
        }

        private static void OpenFile(object sender, EventArgs e)
        {
            if (dirchoice != "" && filechoice != "")
                if (dirchoice == "My Shared Folders" || dirchoice == "My Dump Folders")
                {
                    if (filechoice != "My Dump Folders") //"My shared Folders!"
                        Process.Start(filechoice);
                }
                else if (!Directory.Exists(filechoice))
                {
                    int end = dirchoice.IndexOf("'s shared folders and drives");
                    Program.CallFunc("copy " + filechoice + " from " + dirchoice.Substring(0, end));
                    RefreshWindow();
                    MessageBox.Show(dumps.LastNode.LastNode.Text);
                    Process.Start(dumps.LastNode.LastNode.Text);
                }
                  
        }
        public static void Refresh(object sender, EventArgs e)
        {
            //CopyTreeNodes(tv, q);
            RefreshWindow();
        }

        public static void RefreshWindow()
        {
            string dirs = Program.CallFunc("shared folders");
            dirs = AddInsides(dirs);
            string[] direcs = dirs.Split(new char[] { '\n' });
            tv.Nodes.Clear();
            BuildTree(direcs);
        }
        public static void DeleteFile(object sender, EventArgs e)
        {
            if (dirchoice != "" && filechoice != "")
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete the file?", "Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    if ((dirchoice == "My Shared Folders" || dirchoice == "My Dump Folders") && (filechoice != "My Shared Folders" && filechoice != "My Dump Folders"))
                        File.Delete(filechoice);

                    else 
                        Program.CallFunc("delete " + filechoice + " on " + dirchoice.Substring(0, dirchoice.IndexOf("'s shared folders and drives")));

                    if (dirchoice == "My Dump Folders")
                        foreach (TreeNode node in dumps.Nodes)
                            if (node != null)
                                if (node.Nodes.Count == 0)
                                    Directory.Delete(node.Text);

                    RefreshWindow();
                }
            }
        }

        public static void CreateFile(object sender, EventArgs e)
        {
            string data = Program.CallFunc("clients");
            Create cl = new Create(data.Substring(0, data.IndexOf("stoprightnow")));
            cl.Show();
            RefreshWindow();
        }

        private static void BuildTree(string[] direcs)
        {
            TreeNode tn = new TreeNode();
            TreeNode temp = null;
            tn.Name = "sourcenode";
            tn.Text = "My Shared Folders";
            tv.Nodes.Add(tn);
            TreeNode temp2 = tn;
            bool val = false;
            foreach (string dir in direcs)
            {
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
        private void open_DragEnter(object sender, DragEventArgs e)
        {
            MessageBox.Show("DragEnter!");
            e.Effect = DragDropEffects.Copy;
        }
        private void tv_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }
        private void tv_DragOver(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the mouse position.  
            Point targetPoint = tv.PointToClient(new Point(e.X, e.Y));

            // Select the node at the mouse position.  
            tv.SelectedNode = tv.GetNodeAt(targetPoint);
        }
        private void tv_DragDrop(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the drop location.  
            Point targetPoint = PointToClient(new Point(e.X, e.Y));

            // Retrieve the node that was dragged.  
            string draggedNode = e.Data.GetData("string", true).ToString();
        }
        private void tv_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Move the dragged node when the left mouse button is used.  
            if (e.Button == MouseButtons.Left)
            {
                DoDragDrop(e.Item, DragDropEffects.Move);
            }

            // Copy the dragged node when the right mouse button is used.  
            else if (e.Button == MouseButtons.Right)
            {
                DoDragDrop(e.Item, DragDropEffects.Copy);
            }
        }
        public Form2(string dirs)
        {
            tv = new TreeView();
            SuspendLayout();
            string[] direcs = dirs.Split(new char[] { '\n' });
            BuildTree(direcs);
            
            tv.Font = new Font("Segue", 10);
            tv.Location = new Point(0, 0);
            tv.Size = new Size(402, 380);
            tv.BorderStyle = BorderStyle.FixedSingle;
            tv.AfterSelect += new TreeViewEventHandler(treeView1_AfterSelect);
            tv.ItemDrag += new ItemDragEventHandler(tv_ItemDrag);
            tv.DragEnter += new DragEventHandler(tv_DragEnter);
            tv.DragOver += new DragEventHandler(tv_DragOver);
            tv.DragDrop += new DragEventHandler(tv_DragDrop);
            tv.AllowDrop = true;
            tv.Dock = DockStyle.Fill;
            Controls.Add(tv);

            Font f = new Font("Segue", 12);
            open.Font = f;
            open.Location = new Point(0, 380);
            open.Text = "open";
            open.Click += new EventHandler(OpenFile);
            open.Size = new Size(100, 70);
            open.BackColor = Color.Khaki;
            open.FlatStyle = FlatStyle.Flat;
            open.FlatAppearance.BorderColor = Color.Khaki;
            Controls.Add(open);

            refresh.Font = f;
            refresh.Location = new Point(100, 380);
            refresh.Text = "refresh";
            refresh.Click += new EventHandler(Refresh);
            refresh.Size = new Size(100, 70);
            refresh.BackColor = Color.DeepSkyBlue;
            refresh.FlatStyle = FlatStyle.Flat;
            refresh.FlatAppearance.BorderColor = Color.DeepSkyBlue;
            Controls.Add(refresh);

            create.Font = f;
            create.Location = new Point(200, 380);
            create.Text = "create";
            create.Click += new EventHandler(CreateFile);
            create.Size = new Size(100, 70);
            create.BackColor = Color.MediumSeaGreen;
            create.FlatStyle = FlatStyle.Flat;
            create.FlatAppearance.BorderColor = Color.MediumSeaGreen;
            Controls.Add(create);

            delete.Font = f;
            delete.Location = new Point(300, 380);
            delete.Text = "delete";
            delete.Click += new EventHandler(DeleteFile);
            delete.Size = new Size(100, 70);
            delete.BackColor = Color.Tomato;
            delete.FlatStyle = FlatStyle.Flat;
            delete.FlatAppearance.BorderColor = Color.Tomato;
            Controls.Add(delete);

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            InitializeComponent();
        }
    }
}