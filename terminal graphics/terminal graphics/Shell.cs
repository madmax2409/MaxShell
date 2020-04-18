using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;


namespace terminal_graphics
{
    public partial class Shell : Form
    {
        private static readonly TextBox command = new TextBox();
        private static readonly TextBox output = new TextBox();
        private readonly Stack<string> older = new Stack<string>();
        private readonly Stack<string> newer = new Stack<string>();
        private readonly Button fileman = new Button();
        private readonly Button sysinfo = new Button();
        private readonly Button clist = new Button();
        private static KeyEventArgs enterpress = new KeyEventArgs(Keys.Enter);
        public static string firstdata;

        public static void BlockingInput(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }
        private static void ReadAbout(object sender, EventArgs e)
        {
            PrintOut(Program.CallFunc("about"));
        }
        private void SendEnter(object sender, EventArgs e)
        {
            OutputProcedure(enterpress);
        }
        private void CallOutput(object sender, KeyEventArgs e)
        {
            OutputProcedure(e);
        }
        private void OutputProcedure(KeyEventArgs e) 
        {
            if (e.KeyData == Keys.Enter)
            {
                while (newer.Count > 0) //save command to history
                    older.Push(newer.Pop());

                if (command.Text == "") 
                    return;

                string com = command.Text;
                older.Push(com);
                string outpt = Program.Maintain(com); //send the messsage from the entry to the server

                if (outpt == "")
                    return;

                try { outpt = outpt.Remove(outpt.IndexOf("stoprightnow"), 12); }
                catch { };
                PrintOut(outpt); 
  
                command.Text = "";
            }

            else if (e.KeyData == Keys.Up) //going up and down in command history
            {
                if (older.Count > 0)
                {
                    command.Text = older.Pop();
                    newer.Push(command.Text);
                    command.Select(command.Text.Length, 0);
                }
            }
            else if (e.KeyData == Keys.Down)
            {
                if (newer.Count > 0)
                {
                    command.Text = newer.Pop();
                    older.Push(command.Text);
                    command.Select(command.Text.Length, 0);
                }
            }
        }
        public static void PrintOut(string outpt) 
        {
            string[] dirs = outpt.Split(new char[] { '\n' }); //recieves a string and split sby rows
            for (int i = 0; i < dirs.Length; i++)
            {
                output.AppendText(dirs[i]); 
                output.AppendText(Environment.NewLine);
            }
            output.AppendText(Environment.NewLine);
            
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.ApplicationExitCall)
                return;

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

        private void OpenManager(object sender, EventArgs e)
        {
            Program.Maintain("file manager");
        }

        private void ClientList(object sender, EventArgs e)
        {
            string data = Program.CallFunc("clients");
            ClientList cl = new ClientList(data.Substring(0, data.IndexOf("stoprightnow")));
            cl.Show();
        }

        private void SysInfo(object sender, EventArgs e)
        {
            SystemInfo si = new SystemInfo();
            si.Show();
        }

        
        public static bool DetectMach(string data)
        {
            string[] clients = Program.CallFunc("clients").Split(new char[] { '\n' }); //check wheter the mach passed exists, not an empty value
            foreach (string cl in clients)
            {
                string start = cl.Substring(cl.ToString().IndexOf(", ") + 2);
                string mac = start.Substring(0, start.IndexOf(','));
                if (" " + mac == data)
                    return true;
            }
            return false;
        }

        public Shell()
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.LightSkyBlue;
            Font f = new Font("Segue", 10);
            AllowDrop = true;

            command.Font = f;
            command.Location = new Point(5, 10);
            command.Size = new Size(790, 10);
            command.BorderStyle = BorderStyle.FixedSingle;
            command.KeyDown += new KeyEventHandler(CallOutput);
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
            Button send = new Button
            {
                Font = f,
                Text = "send",
                Size = new Size(95, 50),
                Location = new Point(800, 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.PaleGreen
            };
            send.FlatAppearance.BorderColor = Color.Black;
            send.Click += new EventHandler(SendEnter);
            Controls.Add(send);

            fileman.Font = f;
            fileman.Location = new Point(800, 70);
            fileman.Text = "File Manager";
            fileman.Size = new Size(95, 50);
            fileman.Click += new EventHandler(OpenManager);
            fileman.BackColor = Color.White;
            fileman.FlatStyle = FlatStyle.Flat;
            fileman.FlatAppearance.BorderColor = Color.Black;
            Controls.Add(fileman);

            sysinfo.Font = f;
            sysinfo.Location = new Point(800, 130);
            sysinfo.Text = "System Information";
            sysinfo.Size = new Size(95, 50);
            sysinfo.Click += new EventHandler(SysInfo);
            sysinfo.BackColor = Color.White;
            sysinfo.FlatStyle = FlatStyle.Flat;
            sysinfo.FlatAppearance.BorderColor = Color.Black;
            Controls.Add(sysinfo);

            clist.Font = f;
            clist.Location = new Point(800, 190);
            clist.Text = "Client List";
            clist.Size = new Size(95, 50);
            clist.Click += new EventHandler(ClientList);
            clist.BackColor = Color.White;
            clist.FlatStyle = FlatStyle.Flat;
            clist.FlatAppearance.BorderColor = Color.Black;
            Controls.Add(clist);

            Button about = new Button
            {
                Font = f,
                Text = "About the Creator",
                Location = new Point(800, 250),
                Size = new Size(95, 60),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Khaki
            };
            about.FlatAppearance.BorderColor = Color.Black;
            about.Click += new EventHandler(ReadAbout);
            Controls.Add(about);

            Label sep = new Label
            {
                BorderStyle = BorderStyle.FixedSingle,
                ForeColor = Color.Black,
                BackColor = Color.Black,
                AutoSize = false,
                Location = new Point(5, 455),
                Size = new Size(890, 3)
            };
            Controls.Add(sep);

            GraphIntptr.graph = new Panel
            {
                Location = new Point(5, 470),
                Size = new Size(790, 230),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                AllowDrop = true
            };
            Controls.Add(GraphIntptr.graph);

            GraphIntptr.draggables = new Panel
            {
                Location = new Point(800, 470),
                Size = new Size(95, 230),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            Controls.Add(GraphIntptr.draggables);
            Panel draggable = GraphIntptr.draggables;

            GraphIntptr.FillGraphFuncs();
            GraphIntptr.SetDraggables();

            InitializeComponent();

            PrintOut(firstdata.Remove(firstdata.IndexOf("stoprightnow"), 12));
        }
    }
}
