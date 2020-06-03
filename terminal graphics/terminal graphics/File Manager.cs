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
        private static TreeView tv;
        private static TreeNode dumps;
        public static string filechoice = "";
        public static string dirchoice = "";
        

        private static void ProcessDirectory(string dir, TreeNode Node) // recursive addition of nodes
        {
            try
            {
                string[] SubDir;
                SubDir = Directory.GetFiles(dir);
                foreach (string SB in SubDir)
                {
                    TreeNode tempNode = new TreeNode
                    {
                        Text = SB
                    };
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
                    if (temp != "stoprightnow" && !temp.Contains("an error occurred")) //all the additional files are added before the initiation of the treeview
                    {
                        string subdata = data.Substring(data.IndexOf(mach + "'s shared folders and drives"));
                        int addingindex = subdata.IndexOf(datas[i]);
                        string insertion = "\nfile: " + temp.Substring(0, temp.IndexOf("stoprightnow")) + "\n";
                        bool changed = insertion.Contains("(changed)");
                        if (changed)
                        {
                            insertion = insertion.Replace("(changed)", "");
                            insertion = insertion.Insert(1, "changed ");
                        }
                        //make sure we insert at the right place if identical names are detected
                        data = data.Insert(data.IndexOf(subdata) + addingindex + datas[i].Length, insertion);
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
                    Program.CallFunc("copy " + "'" + filechoice + "' from " + dirchoice.Substring(0, end)); //to copy remote folder, first we reques to copy the file
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
        private static void DeleteFile(object sender, EventArgs e)
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
                    }

                    RefreshWindow();
                }
            }
        }

        private static void CreateFile(object sender, EventArgs e)
        {
            string data = Program.CallFunc("clients");
            Create cl = new Create(data.Substring(0, data.IndexOf("stoprightnow")), "create");
            cl.Show();
        }

        private static void BuildTree(string[] direcs)
        { 
            TreeNode tn = new TreeNode();
            TreeNode temp = null;
            tn.Name = "sourcenode";
            tn.Text = "My Shared Folders";
            tv.Nodes.Add(tn);
            TreeNode temp2 = tn;
            string client = Environment.MachineName;
            for (int i =0; i <direcs.Length;i++)
            {
                if (direcs[i].Length > 3 && direcs[i] != "stoprightnow" && direcs[i] != "C:\\MaxShell\\dump_folders") //build the tree in order of: first local shared, then all other clients
                {
                    if (direcs[i].Contains("'s shared folders and drives"))  //check if it's a new client
                        if (direcs[i].Substring(0, direcs[i].IndexOf("'s shared folders and drives")) != Environment.MachineName)
                        {
                            temp2 = new TreeNode
                            {
                                Name = direcs[i],
                                Text = direcs[i]
                            };
                            tv.Nodes.Add(temp2);
                            client = direcs[i].Substring(0, direcs[i].IndexOf("'s shared folders and drives"));
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

                        bool changed = direcs[i].Contains("changed file: ");

                        if (client == Environment.MachineName) //check if the folder/file are local
                        { 
                            temp = temp2.Nodes.Add(direcs[i]); //recurisve build if local
                            ProcessDirectory(direcs[i], temp);
                        }
                        else if (!direcs[i].Contains("stoprightnow")) //if remote, all the files are next in the array
                        {
                            if (path != direcs[i]) //add to the last saved node if it's a file
                            {
                                TreeNode node = new TreeNode(path);
                                if (changed)
                                    node.ForeColor = Color.Red;
                                temp.Nodes.Add(node);
                            }
                            else
                                temp = temp2.Nodes.Add(direcs[i]); //create a new node if it's a folder
                        }
                    }
                }
            }
            ShowDumps(); //add the dump folders at the end
        }

        private static void ShowDumps()
        {
            if (Directory.Exists("C:\\MaxShell\\dump_folders")) //create the dump folders directory if it's not in place and add all the dumps to the tree
            {
                dumps = new TreeNode("My Dump Folders");
                tv.Nodes.Add(dumps);
                foreach (string dir in Directory.GetFileSystemEntries("C:\\MaxShell\\dump_folders"))
                {
                    TreeNode temp = new TreeNode(dir);
                    dumps.Nodes.Add(temp);
                    ProcessDirectory(dir, temp);
                }
            }
            else
                Directory.CreateDirectory("C:\\MaxShell\\dump_folders");
        }

        private void CreateButtons()
        {
            Font f = new Font("Segue", 12);
            Color[] colors = new Color[] { Color.Khaki, Color.DeepSkyBlue, Color.MediumSeaGreen, Color.Tomato };
            string[] buttons = new string[] { "open", "refresh", "create", "delete" };
            int x = 0, y = 380;
            for(int i =0; i<buttons.Length; i++)
            {
                Button btn = new Button {
                    Font = f,
                    Size = new Size(114, 70),
                    Text = buttons[i],
                    BackColor = colors[i],
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(x, y)
                };
                btn.FlatAppearance.BorderColor = colors[i];
                x += 114;
                switch (buttons[i])
                {
                    case "open":
                        btn.Click += new EventHandler(OpenFile);
                        break;
                    case "refresh":
                        btn.Click += new EventHandler(Refresh);
                        break;
                    case "create":
                        btn.Click += new EventHandler(CreateFile);
                        break;
                    case "delete":
                        btn.Click += new EventHandler(DeleteFile);
                        break;
                }
                Controls.Add(btn);
            }
        }

        public Form2(string dirs)
        {
            tv = new TreeView();
            tv.BorderStyle = BorderStyle.FixedSingle;
            string[] direcs = dirs.Split(new char[] { '\n' });
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            MessageBox.Show(dirs);
            BuildTree(direcs);
            
            tv.Font = new Font("Segue", 10);
            tv.Location = new Point(0, 0);
            tv.Size = new Size(456, 380);
            tv.BorderStyle = BorderStyle.FixedSingle;
            tv.AfterSelect += new TreeViewEventHandler(tv_AfterSelect);
            Controls.Add(tv);

            CreateButtons();

            InitializeComponent();
        }
    }
}