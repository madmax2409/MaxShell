using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;


namespace terminal_graphics
{
    public partial class Shell : Form
    {
        private Label direc = new Label();
        private TextBox command = new TextBox();
        private TextBox output = new TextBox();
        private Stack<string> older = new Stack<string>();
        private Stack<string> newer = new Stack<string>();
        private Button fileman = new Button();
        private Button sysinfo = new Button();
        private Button clist = new Button();

        private void BlockingInput(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void OutputProcedure(object sender, KeyEventArgs e)
        {
            TextBox cmd = (TextBox)sender;
            if (e.KeyData == Keys.Enter)
            {
                while (newer.Count > 0)
                    older.Push(newer.Pop());

                if (cmd.Text == "")
                    return;

                string command = cmd.Text;
                older.Push(command);
                string outpt = Program.Maintain(command);
                if (outpt == "")
                    return;
                string[] dirs = outpt.Split(new char[] { '\n' });
                for (int i = 0; i < dirs.Length; i++)
                {
                    output.AppendText(dirs[i]);
                    output.AppendText(Environment.NewLine);
                }
                output.AppendText(Environment.NewLine);
                cmd.Text = "";
            }

            else if (e.KeyData == Keys.Up)
            {
                if (older.Count > 0)
                {
                    cmd.Text = older.Pop();
                    newer.Push(cmd.Text);
                    cmd.Select(cmd.Text.Length, 0);
                }
            }
            else if (e.KeyData == Keys.Down)
            {
                if (newer.Count > 0)
                {
                    cmd.Text = newer.Pop();
                    older.Push(cmd.Text);
                    cmd.Select(cmd.Text.Length, 0);
                }
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.ApplicationExitCall)
            {
                return;
            }

            switch (MessageBox.Show(this, "Are you sure you want to exit?", "Exiting", MessageBoxButtons.YesNo))
            {
                case DialogResult.No:
                    e.Cancel = true;
                    break;
                default:
                    if (Program.form2 != null)
                        Program.form2.Close();
                    break;
            }
        }

        public void OpenManager(object sender, EventArgs e)
        {
            Program.Maintain("file manager");
        }

        public void ClientList(object sender, EventArgs e)
        {
            string data = Program.CallFunc("clients");
            ClientList cl = new ClientList(data.Substring(0, data.IndexOf("stoprightnow")));
            cl.Show();
        }

        public void SysInfo(object sender, EventArgs e)
        {
            SystemInfo si = new SystemInfo();
            si.Show();
        }
        public Shell()
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.LightSkyBlue;


            Font f = new Font("Segue", 10);
            /*direc.Font = f;
            direc.Location = new Point(0, 10);
            string dir = Directory.GetCurrentDirectory();
            char[] seperate = { '\\' };
            string[] dirs = dir.Split(seperate);
            if (dirs.Length > 3)
                dir = dirs[0] + @"\" + dirs[1] + @"\" + dirs[2] + @"\...\" + dirs[dirs.Length - 1] + ">";
            direc.Text = dir;
            SizeF size = direc.CreateGraphics().MeasureString(direc.Text, direc.Font);
            direc.Size = new Size((int)size.Width + 10, (int)size.Height + 2);
            Controls.Add(direc);
            */
            command.Font = f;
            command.Location = new Point(5, 10);
            command.Size = new Size(890, 10);
            command.BorderStyle = BorderStyle.FixedSingle;
            command.KeyDown += new KeyEventHandler(OutputProcedure);
            Controls.Add(command);

            f = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);
            output.Font = f;
            output.Location = new Point(5, 40);
            output.Multiline = true;
            output.Size = new Size(790, 405);
            output.BorderStyle = BorderStyle.Fixed3D;
            output.ScrollBars = ScrollBars.Both;
            output.BackColor = Color.Black;
            output.ForeColor = Color.LightSkyBlue;
            output.KeyPress += new KeyPressEventHandler(BlockingInput);
            Controls.Add(output);

            f = new Font("Segue", 10);
            fileman.Font = f;
            fileman.Location = new Point(800, 40);
            fileman.Text = "File Manager";
            fileman.Size = new Size(95, 50);
            fileman.Click += new EventHandler(OpenManager);
            fileman.BackColor = Color.White;
            fileman.FlatStyle = FlatStyle.Flat;
            fileman.FlatAppearance.BorderColor = Color.Black;
            Controls.Add(fileman);

            sysinfo.Font = f;
            sysinfo.Location = new Point(800, 100);
            sysinfo.Text = "System Information";
            sysinfo.Size = new Size(95, 50);
            sysinfo.Click += new EventHandler(SysInfo);
            sysinfo.BackColor = Color.White;
            sysinfo.FlatStyle = FlatStyle.Flat;
            sysinfo.FlatAppearance.BorderColor = Color.Black;
            Controls.Add(sysinfo);

            clist.Font = f;
            clist.Location = new Point(800, 160);
            clist.Text = "Client List";
            clist.Size = new Size(95, 50);
            clist.Click += new EventHandler(ClientList);
            clist.BackColor = Color.White;
            clist.FlatStyle = FlatStyle.Flat;
            clist.FlatAppearance.BorderColor = Color.Black;
            Controls.Add(clist);

            InitializeComponent();
        }
    }
}
