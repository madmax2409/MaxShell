using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.IO;

namespace terminal_graphics
{
    public partial class Form2 : Form
    {
        private void ProcessDirectory(string Dir, TreeNode Node)
        {

            try
            {
                string[] SubDir;
                SubDir = Directory.GetDirectories(Dir);
                foreach (string SB in SubDir) // exit upon empty 
                {
                    TreeNode tempNode = new TreeNode(SB); ProcessDirectory(SB, tempNode); // recursive call per node
                    Node.Nodes.Add(tempNode);
                }
            }
            catch (UnauthorizedAccessException e)
            {

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
                if (!file["Name"].ToString().Contains("Windows"))
                {
                    TreeNode tn = new TreeNode(file["Name"].ToString());
                    ProcessDirectory(file["path"].ToString(), tn);
                }
            }
            Controls.Add(tv);
        
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            InitializeComponent();
        }
    }
}
