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
        private static TreeNode dumps;
        private static Queue<bool> q = new Queue<bool>();

        private static void ProcessDirectory(string dir, TreeNode Node) // recursive addition of nodes
        {
            try
            {
                string[] SubDir;
                SubDir = Directory.GetFiles(dir);
                foreach (string SB in SubDir)
                {
                    TreeNode tempNode = new TreeNode();
                    tempNode.Text = SB;
                    Node.Nodes.Add(tempNode);
                    ProcessDirectory(SB, tempNode);
                }
            }
            catch 
            {
                
            }
        }
        private static void tv_AfterSelect(object sender, TreeViewEventArgs e) //set parameters on each time a treenode is selceted
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
                filechoice = e.Node.Text;
            }
        }
        public static string AddInsides(string data) //add insides of remote folders
        {
            string temp, mach = "";
            string[] datas = data.Split(new char[] { '\n' });
            for(int i = 0; i<datas.Length;i++)
            {
                //check if the folder is remote and retrieve the machine name of the computer
                if (datas[i].IndexOf(Environment.MachineName) == -1 && datas[i].IndexOf("'s shared folders and drives") != -1) 
                    mach = datas[i].Substring(0, datas[i].IndexOf("'s shared folders and drives"));

                else if (mach != "" && mach != Environment.MachineName) //if a machine name is deteced, a request to list the files in the folder is sent
                {
                    temp = Program.CallFunc("list " + datas[i] + " on " + mach);
                    if (temp != "stoprightnow")
                    {   //all the additional files are added before the initiation of the treeview
                        //string direcs[] = data.Split(new char[] )
                        data = data.Insert(data.IndexOf(datas[i]) + datas[i].Length, "\nfile: " + datas[i] + "\\" + temp.Substring(0, temp.IndexOf("stoprightnow")));
                    }
                }
            }
            return data;
        }

        private static void OpenFile(object sender, EventArgs e)
        {
            if (dirchoice != "" && filechoice != "")
                if ((dirchoice == "My Shared Folders" || dirchoice == "My Dump Folders") && (filechoice != "My Dump Folders" && filechoice != "My Shared Folders"))
                        Process.Start(filechoice); //in case it's local, we open it right away

                else if (dirchoice != "My Shared Folders" && dirchoice != "My Dump Folders")
                {
                    int end = dirchoice.IndexOf("'s shared folders and drives"); 
                    Program.CallFunc("copy " + filechoice + " from " + dirchoice.Substring(0, end)); //to copy remote folder, first we reques to copy the file
                    RefreshWindow();
                    Process.Start(dumps.LastNode.LastNode.Text); //and then start the process in the last node, the last copied one
                }
                  
        }
        public static void Refresh(object sender, EventArgs e)
        {
            RefreshWindow();
        }

        public static void RefreshWindow()
        {
            string dirs = Program.CallFunc("shared folders"); //request the data to update changes
            dirs = AddInsides(dirs);
            string[] direcs = dirs.Split(new char[] { '\n' });
            tv.Nodes.Clear();
            BuildTree(direcs); //and rebuild the tree
        }
        public static void DeleteFile(object sender, EventArgs e)
        {
            if (dirchoice != "" && filechoice != "")
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete the file?", "Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    if (!Directory.Exists(filechoice))
                    {
                        //if the file is local we can delete it right away, if not - we request the deletion from the server
                        if ((dirchoice == "My Shared Folders" || dirchoice == "My Dump Folders") && (filechoice != "My Shared Folders" && filechoice != "My Dump Folders"))
                            File.Delete(filechoice);

                        else
                            Program.CallFunc("delete " + filechoice + " on " + dirchoice.Substring(0, dirchoice.IndexOf("'s shared folders and drives")));

                        if (dirchoice == "My Dump Folders") //delete empty dump folders
                            foreach (TreeNode node in dumps.Nodes)
                                if (node != null)
                                    if (node.Nodes.Count == 0)
                                        Directory.Delete(node.Text);
                    }
                    else
                    {
                        if ((dirchoice == "My Shared Folders" || dirchoice == "My Dump Folders") && (filechoice != "My Shared Folders" && filechoice != "My Dump Folders"))
                            Directory.Delete(filechoice, true);

                        else
                            MessageBox.Show("Recursive Deletion to be added");
                    }

                    RefreshWindow();
                }
            }
        }

        public static void CreateFile(object sender, EventArgs e)
        {
            string data = Program.CallFunc("clients");
            Create cl = new Create(data.Substring(0, data.IndexOf("stoprightnow")));
            cl.Show();
        }

        private static void BuildTree(string[] direcs)
        {
            for (int i = 0; i < direcs.Length; i++)
                MessageBox.Show(direcs[i]);

            TreeNode tn = new TreeNode();
            TreeNode temp = null;
            tn.Name = "sourcenode";
            tn.Text = "My Shared Folders";
            tv.Nodes.Add(tn);
            TreeNode temp2 = tn;
            for (int i =0; i <direcs.Length;i++)
            {
                if (direcs[i].Length > 3 && direcs[i] != "stoprightnow" && direcs[i] != "C:\\dump_folders") //build the tree in order of: first local shared, then all other clients
                {
                    if (direcs[i].Contains("'s shared folders and drives"))  //check if it's a new client
                        if (direcs[i].Substring(0, direcs[i].IndexOf("'s shared folders and drives")) != Environment.MachineName)
                        {
                            temp2 = new TreeNode();
                            temp2.Name = direcs[i];
                            temp2.Text = direcs[i];
                            tv.Nodes.Add(temp2);
                        }
                        else
                            continue;
                    else
                    {
                        string path; //determine wheter the string is a folder or a file
                        if (direcs[i].Contains("file: "))
                            path = direcs[i].Substring(direcs[i].IndexOf("file: ") + 6);
                        else
                            path = direcs[i];

                        if (Directory.Exists(path) || File.Exists(path)) //check if the folder/file are local
                        { 
                            temp = temp2.Nodes.Add(direcs[i]); //recurisve build if local
                            ProcessDirectory(direcs[i], temp);
                        }
                        else if (!direcs[i].Contains("stoprightnow")) //if remote, all the files are next in the array
                        {
                            if (direcs[i].Contains("file: ")) //add to the last saved node if it's a file
                                temp.Nodes.Add(path);

                            else
                                temp = temp2.Nodes.Add(direcs[i]); //create a new node if it's a folder
                        }
                    }
                }
            }
            ShowDumps(); //add the dump folders at the end
        }

        public static void ShowDumps()
        {
            if (Directory.Exists("C:\\dump_folders")) //create the dump folders directory if it's not in place and add all the dumps to the tree
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
            tv = new TreeView();
            string[] direcs = dirs.Split(new char[] { '\n' });
            BuildTree(direcs);
            
            tv.Font = new Font("Segue", 10);
            tv.Location = new Point(0, 0);
            tv.Size = new Size(402, 380);
            tv.BorderStyle = BorderStyle.FixedSingle;
            tv.AfterSelect += new TreeViewEventHandler(tv_AfterSelect);
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