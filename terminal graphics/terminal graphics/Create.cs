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
        private static readonly TextBox filename = new TextBox();
        private static readonly ComboBox exts = new ComboBox();
        private static readonly ComboBox machs = new ComboBox();
        private static readonly Button ok = new Button();
        private static readonly Button cancel = new Button();
        private static string[] pcnames;

        private void OutCreate(object sender, EventArgs e)
        {
            bool flag = Creation();
            if (flag)
                Close();
        }
        private static bool Creation()
        {
            string file = filename.Text + "." + exts.SelectedItem; //get the file name and chosen extension from the textbox and combobox of the window
            string choice = machs.SelectedItem.ToString();         //get the chosen target from the combobox of connected computers
            string start = choice.Substring(choice.IndexOf(", ") + 2);
            string mach = start.Substring(0, start.IndexOf(',')); //get the machine name from the client list
            string result = Program.CallFunc("create '" + file + "' on " + mach); //send the creation request

            if (result.IndexOf("created") != -1)
            {
                MessageBox.Show("Created the file!", "Sucess");
                return true;
            }
            else
            {
                MessageBox.Show("Something is wong with your choice, please try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void Close_Window(object sender, EventArgs a)
        {
            Close();
        }

        public Create(string data)
        {
            Font f = new Font("Segue", 10);
            pcnames = data.Split(new char[] { '\n' });

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

            machs.DropDownStyle = ComboBoxStyle.DropDownList;
            machs.Location = new Point(60, 10);
            machs.Size = new Size(230, 10);
            machs.Items.Add("Choose a Client");
            foreach (string item in pcnames)
                machs.Items.Add(item);
            machs.SelectedItem = machs.Items[0];
            Controls.Add(machs);

            Label file = new Label
            {
                Text = "File:",
                Font = f,
                Location = new Point(5, 42),
                Size = new Size(45, 15)
            };
            Controls.Add(file);

            filename.Location = new Point(60, 40);
            Controls.Add(filename);

            exts.DropDownStyle = ComboBoxStyle.DropDownList;
            exts.Location = new Point(169, 40);
            exts.Items.Add("File Extension");
            foreach (string item in new string[] { "txt", "docx", "pdf" })
                exts.Items.Add(item);
            exts.SelectedItem = exts.Items[0];
            Controls.Add(exts);

            ok.Font = new Font("Segue", 10);
            ok.Text = "create";
            ok.Location = new Point(60, 65);
            ok.Click += new EventHandler(OutCreate);
            ok.BackColor = Color.PaleGreen;
            ok.Size = new Size(80, 25);
            ok.FlatStyle = FlatStyle.Flat;
            ok.FlatAppearance.BorderColor = Color.Black;
            Controls.Add(ok);

            cancel.Font = new Font("Segue", 10);
            cancel.Text = "cancel";
            cancel.Location = new Point(210, 65);
            cancel.Click += new EventHandler(Close_Window);
            cancel.BackColor = Color.LightCoral;
            cancel.Size = new Size(80, 25);
            cancel.FlatStyle = FlatStyle.Flat;
            cancel.FlatAppearance.BorderColor = Color.Black;
            Controls.Add(cancel);

            InitializeComponent();
        }
    }
}

