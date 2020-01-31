using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
<<<<<<< HEAD
using System.Management;
using System.IO;
=======
>>>>>>> a0ae94e76c26a9289fafbd93cb8d70a427b18ff6

namespace terminal_graphics
{
    public partial class Form2 : Form
    {
<<<<<<< HEAD
        private void ProcessDirectory(string Dir, TreeNode Node)
        {
            string[] SubDir; 
            SubDir = Directory.GetDirectories(Dir); 
            foreach (string SB in SubDir) // exit upon empty 
            { 
                TreeNode tempNode = new TreeNode(SB);ProcessDirectory(SB, tempNode); // recursive call per node
                Node.Nodes.Add(tempNode);
            }
        }
        public Form2()
        {
            TreeView tv = new TreeView();
            tv.Nodes.Clear(); 
            
            tv.Font = new Font("comic sans", 10);
            tv.Location = new Point(0,0);
            tv.Size = new Size(400, 450);
            tv.BorderStyle = BorderStyle.FixedSingle;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("\\\\" + Environment.MachineName + "\\root\\CIMV2", "SELECT * FROM Win32_Share");
            foreach (ManagementObject file in searcher.Get())
            {
                TreeNode tn = new TreeNode(file["Name"].ToString());
                ProcessDirectory(file["path"].ToString(), tn);
            }
            Controls.Add(tv);
=======
        TreeView tv = new TreeView();
        public Form2()
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

>>>>>>> a0ae94e76c26a9289fafbd93cb8d70a427b18ff6
            InitializeComponent();
        }
    }
}
