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
        private static TextBox filename = new TextBox();
        private static ComboBox exts = new ComboBox();
        private static ComboBox machs = new ComboBox();
        private static Button ok = new Button();
        private static Button cancel = new Button();
        private static int counter = 0;
        public static string[] pcnames;

        public void OutCreate(object sender, EventArgs e)
        {
            bool flag = Creation();
            if (flag)
                Close();
        }
        public static bool Creation()
        {
            try
            {
                string file = filename.Text + "." + exts.SelectedItem;
                if (!Directory.Exists("C:\\dump_folders\\local_dump"))
                    Directory.CreateDirectory("C:\\dump_folders\\local_dump");

                File.Create("C:\\dump_folders\\local_dump\\" + file);
                MessageBox.Show("File Created in local dumps!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return true;
            }
            catch (IOException)
            {
                counter++;
                return Creation();
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Something is wong with your choice, please try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void Close_Window(object sender, EventArgs a)
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

            Label target = new Label();
            target.Font = f;
            target.Text = "Target:";
            target.Location = new Point(5, 12);
            target.Size = new Size(55, 17);
            Controls.Add(target);

            machs.DropDownStyle = ComboBoxStyle.DropDownList;
            machs.Location = new Point(60, 10);
            machs.Size = new Size(230, 10);
            foreach (string item in pcnames)
                machs.Items.Add(item);
            Controls.Add(machs);

            Label file = new Label();
            file.Text = "File:";
            file.Font = f;
            file.Location = new Point(5, 42);
            file.Size = new Size(45, 15);
            Controls.Add(file);

            filename.Location = new Point(60, 40);
            Controls.Add(filename);

            exts.DropDownStyle = ComboBoxStyle.DropDownList;
            exts.Location = new Point(169, 40);
            foreach (string item in new string[] { "txt", "docx", "pdf" })
                exts.Items.Add(item);
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
