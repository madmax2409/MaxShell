using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace terminal_graphics
{
    public partial class Create : Form
    {
        private static TextBox filename;
        private static ComboBox exts;
        private static ComboBox machs;
        private static Button ok;
        private static Button cancel;
        private static string[] pcnames;
        private static string funcname;

        private void OutFunc(object sender, EventArgs e)
        {
            
            string param = filename.Text;  
            string choice = machs.SelectedItem.ToString();  //get the chosen target from the combobox of connected computers
            string start = choice.Substring(choice.IndexOf(", ") + 2); //select exactly the mach from the string
            string mach = start.Substring(0, start.IndexOf(',')); //get the machine name from the client list
            bool flag = false;
            switch (funcname)
            {
                case "create":
                    flag = Creation(mach);
                    break;
                case "delete":
                    flag = Deletion(mach, param);
                    break;
                case "run":
                    flag = StartProc(mach, param);
                    break;
                case "kill":
                    flag = KillProc(mach, param);
                    break;
            }
            if (flag)
                Close();
        }

        private static bool Creation(string mach)
        {
            if (filename.Text != "" && exts.SelectedItem.ToString() != "File Extension")
            {
                string file = filename.Text + "." + exts.SelectedItem;
                string result = Program.CallFunc("create '" + file + "' on " + mach); //send the creation request

                if (result.IndexOf("created") != -1)
                {
                    MessageBox.Show(result.Remove(result.IndexOf("stoprightnow"), 12), "Sucess");
                    return true;
                }
                else
                {
                    MessageBox.Show("Something is wrong with your choice, please try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return false;
        }

        private static bool Deletion(string mach, string path)
        {
            string result = Program.CallFunc("delete '" + path + "' on " + mach); //send the deletion request
            if (result.IndexOf("deleted") != -1)
            {
                MessageBox.Show(result.Remove(result.IndexOf("stoprightnow"), 12), "Sucess");
                return true;
            }
            else
            {
                MessageBox.Show("Something is wrong with your choice, please try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static bool StartProc(string mach, string path)
        {
            string result = Program.CallFunc("run '" + path + "' on " + mach); //send the run request
            if (result.IndexOf("created") != -1)
            {
                MessageBox.Show(result.Remove(result.IndexOf("stoprightnow"), 12), "Sucess");
                return true;
            }
            else
            {
                MessageBox.Show("Something is wrong with your choice, please try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static bool KillProc(string mach, string path)
        {
            string result = Program.CallFunc("kill '" + path + "' on " + mach); //send the kill request
            if (result.IndexOf("Terminated") != -1)
            {
                MessageBox.Show(result.Remove(result.IndexOf("stoprightnow"), 12), "Sucess");
                return true;
            }
            else
            {
                MessageBox.Show("Something is wrong with your choice, please try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void Close_Window(object sender, EventArgs a)
        {
            Close();
        }

        private void CreationWindow()
        {
            Label file = new Label
            {
                Text = "File:",
                Font = new Font("Segue", 10),
                Location = new Point(5, 42),
                Size = new Size(45, 15)
            };
            Controls.Add(file);

            filename.Location = new Point(80, 40);
            filename.Width = 95;
            Controls.Add(filename);

            exts = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(185, 40),
                Width = 105
            };
            exts.Items.Add("File Extension");
            foreach (string item in new string[] { "txt", "docx", "pdf" })
                exts.Items.Add(item);
            exts.SelectedItem = exts.Items[0];
            Controls.Add(exts);
        }
        private void DeletionWindow()
        {
            Label file = new Label
            {
                Text = "Path:",
                Font = new Font("Segue", 10),
                Location = new Point(5, 42),
                Size = new Size(45, 15)
            };
            Controls.Add(file);
        }

        private void RunWindow()
        {
            Label file = new Label
            {
                Text = "Process:",
                Font = new Font("Segue", 10),
                Location = new Point(5, 42),
                Size = new Size(70, 25)
            };
            Controls.Add(file);
        }

        public Create(string data, string typeofwinodw, string mach = "")
        {
            try { data = data.Remove(data.IndexOf("stoprightnow"), 12); }
            catch { };
            Font f = new Font("Segue", 10);
            filename = new TextBox();
            pcnames = data.Split(new char[] { '\n' });
            funcname = typeofwinodw;

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.LightSkyBlue;

            Label target = new Label
            {
                Font = f,
                Text = "Target:",
                Location = new Point(5, 12),
                Size = new Size(55, 17)
            };
            Controls.Add(target);

            machs = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(80, 10),
                Size = new Size(210, 10)
            };
            foreach (string item in pcnames)
                machs.Items.Add(item);
            if (mach == "")
            {
                machs.Items.Add("Choose a Client");
                machs.SelectedItem = machs.Items[0];
            }
            else
                foreach (object item in machs.Items)
                {
                    string start = item.ToString().Substring(item.ToString().IndexOf(", ") + 2);
                    string mac = start.Substring(0, start.IndexOf(','));
                    if (" " + mac == mach)
                    {
                        machs.SelectedItem = item;
                        break;
                    }
                }
            Controls.Add(machs);

            ok = new Button
            {
                Font = new Font("Segue", 10),
                Location = new Point(80, 65),
                BackColor = Color.PaleGreen,
                Size = new Size(80, 25),
                FlatStyle = FlatStyle.Flat
            };
            ok.Click += new EventHandler(OutFunc);
            ok.FlatAppearance.BorderColor = Color.Black;
            switch (typeofwinodw)
            {
                case "create":
                    Text = "Create A File";
                    ok.Text = "Create";
                    CreationWindow();
                    break;

                case "delete":
                    Text = "Delete A File";
                    ok.Text = "Delete";
                    DeletionWindow();
                    filename.Location = new Point(80, 40);
                    filename.Width = 210;
                    Controls.Add(filename);
                    break;

                case "run":
                    Text = "Run A Process";
                    ok.Text = "Run";
                    RunWindow();
                    filename.Location = new Point(80, 40);
                    filename.Width = 210;
                    Controls.Add(filename);
                    break;

                case "kill":
                    Text = "Kill A Process";
                    ok.Text = "Kill";
                    RunWindow();
                    filename.Location = new Point(80, 40);
                    filename.Width = 210;
                    Controls.Add(filename);
                    break;
            }
            Controls.Add(ok);

            cancel = new Button
            {
                Font = new Font("Segue", 10),
                Text = "cancel",
                Location = new Point(210, 65),
                BackColor = Color.LightCoral,
                Size = new Size(80, 25),
                FlatStyle = FlatStyle.Flat
            };
            cancel.Click += new EventHandler(Close_Window);
            cancel.FlatAppearance.BorderColor = Color.Black;
            Controls.Add(cancel);

            InitializeComponent();
        }
    }
}

