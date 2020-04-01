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
        private static Button ok = new Button();
        private static Button cancel = new Button();
        private static int counter = 0;

        public static void OutCreate(object sender, EventArgs e)
        {
            Creation();
        }
        public static void Creation()
        {
            try
            {
                string file = filename.Text + "." + exts.SelectedItem;
                if (!Directory.Exists("C:\\dump_folders\\local_dump"))
                    Directory.CreateDirectory("C:\\dump_folders\\local_dump");

                File.Create("C:\\dump_folders\\local_dump\\" + file);
                MessageBox.Show("File Created in local dumps!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (IOException)
            {
                counter++;
                Creation();
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Something is wong with your choice, please try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Close_Window(object sender, EventArgs a)
        {
            Close();
        }
        public Create()
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            filename.Location = new Point(20, 20);
            Controls.Add(filename);

            exts.DropDownStyle = ComboBoxStyle.DropDownList;
            exts.Location = new Point(130, 20);
            foreach (string item in new string[] { "txt", "docx", "pdf" })
                exts.Items.Add(item);
            Controls.Add(exts);

            ok.Font = new Font("comic sans", 10);
            ok.Text = "create";
            ok.Location = new Point(20, 50);
            ok.Click += new EventHandler(OutCreate);
            Controls.Add(ok);

            cancel.Font = new Font("comic sans", 10);
            cancel.Text = "cancel";
            cancel.Location = new Point(110, 50);
            cancel.Click += new EventHandler(Close_Window);
            Controls.Add(cancel);

            InitializeComponent();
        }
    }
}
