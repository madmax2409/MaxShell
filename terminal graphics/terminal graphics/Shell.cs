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
        private static readonly TextBox choice = new TextBox();
        private readonly Stack<string> older = new Stack<string>();
        private readonly Stack<string> newer = new Stack<string>();
        private readonly Button fileman = new Button();
        private readonly Button sysinfo = new Button();
        private readonly Button clist = new Button();
        private static Panel graph;
        private static Panel draggables;
        private static readonly ComboBox pcnames = new ComboBox();
        private static readonly Dictionary<string, string> funcnames = new Dictionary<string, string>();
        private static readonly string[] st = new string[] { "get cpu", "get ram", "get windows", "run", "kill", "free space", "path", "list processes", "create", "delete" };

        private static void BlockingInput(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void OutputProcedure(object sender, KeyEventArgs e) 
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
        private static void PrintOut(string outpt)
        {
            try { outpt = outpt.Remove(outpt.IndexOf("stoprightnow"), 12); }
            catch { };
            string[] dirs = outpt.Split(new char[] { '\n' });
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

        private static void TextMouseDown(object sender, MouseEventArgs e)
        {
            
            if (e.Button == MouseButtons.Left) // Start the drag if it's the right mouse button.
                choice.DoDragDrop(choice.Text, DragDropEffects.Copy);
        }

       
        private static void Button_DragEnter(object sender, DragEventArgs e)  // Indicate that we can accept a copy of text
        {
            // See if this is a copy and the data includes text.
            if (e.Data.GetDataPresent(DataFormats.Text) && (e.AllowedEffect & DragDropEffects.Copy) != 0)
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None; // Don't allow any other drop.
        } 
        private static void OpenDraggedFile(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(DataFormats.FileDrop);
            MessageBox.Show(data.ToString());
        }
            private static void Button_DragDrop(object sender, DragEventArgs e)
        {
            string data = (string)e.Data.GetData(DataFormats.Text);
            string command = ((Button)sender).Text;
            foreach (KeyValuePair<string, string> pair in funcnames)
                if (pair.Key == command)
                {
                    if (pair.Value == "create" || pair.Value == "delete" || pair.Value == "kill" || pair.Value == "run")
                    {
                        if (DetectMach(data))
                            Program.Maintain(pair.Value, data);
                    }
                    else
                        data = Program.Maintain(pair.Value + " on" + data);

                    PrintOut(data);
                    break;
                }
        }

        private static bool DetectMach(string data)
        {
            string[] clients = Program.CallFunc("clients").Split(new char[] { '\n' });
            foreach (string cl in clients)
            {
                string start = cl.Substring(cl.ToString().IndexOf(", ") + 2);
                string mac = start.Substring(0, start.IndexOf(','));
                if (" " + mac == data)
                    return true;
            }
            return false;
        }

        private static void BuildRow(string[] butnames, int y)
        {
            int x = 0;
            for (int i = 0; i < butnames.Length; i++)
            {
                Button temp = new Button();
                string text = butnames[i];
                text = text.Insert(text.IndexOf('['), "\n");
                if (y == 70)
                    funcnames.Add(text, st[i]);
                else
                    funcnames.Add(text, st[i + 5]);
                temp.Text = text;
                temp.Font = new Font("Segue", 12);
                temp.FlatStyle = FlatStyle.Flat;
                temp.FlatAppearance.BorderColor = Color.Black;
                temp.AllowDrop = true;
                temp.DragEnter += new DragEventHandler(Button_DragEnter);
                if (text == "Path")
                    temp.DragDrop += new DragEventHandler(OpenDraggedFile);
                else
                    temp.DragDrop += new DragEventHandler(Button_DragDrop);
                if (i < 3)
                {
                    temp.Size = new Size(130, 80);
                    temp.Location = new Point(x, y);
                    x += 130;
                }
                else
                {
                    temp.Size = new Size(200, 80);
                    temp.Location = new Point(x, y);
                    x += 200;
                }
                graph.Controls.Add(temp);
            }
        }

        private static void FillGraphFuncs()
        {
            Font f = new Font("Segue", 12);
            Label info = new Label
            {
                Font = f,
                Text = "To initiate a function, drag the right parameter to the button space",
                Size = new Size(790, 40),
                BorderStyle = BorderStyle.FixedSingle
            };
            graph.Controls.Add(info);

            string[] sublabels = { "Data", "Action" };
            int x = 0, y = 40, size = 390;
            foreach (string lab in sublabels)
            {
                Label temp = new Label
                {
                    Text = lab,
                    Font = f,
                    Size = new Size(size, 30),
                    Location = new Point(x, y),
                    BorderStyle = BorderStyle.FixedSingle
                };
                graph.Controls.Add(temp);
                x += 390;
                size += 10;
            }
            BuildRow(new string[] {"CPU [PC Name]", "RAM [PC Name]", "Windows Ver. [PC Name]", "Run [PC Name]", "Kill [PC Name]" }, 70);
            BuildRow(new string[] { "Free Space [PC Name]", "Path [File]", "Processes [PC Name]", "Create [PC Name]", "Delete [File]" }, 150);
        }

        private static void SetChoice(object sender, EventArgs e)
        {
            if (pcnames.SelectedItem.ToString() != "Target PC")
                choice.Text = pcnames.SelectedItem.ToString();
        }

        private static void SetDraggables()
        {
            string[] clients = Program.CallFunc("clients").Split(new char[] { '\n' });
            string[] machs = new string[clients.Length];
            for (int i = 0; i< clients.Length; i++)
                machs[i] = clients[i].Split(new char[] { ',' })[1];

            pcnames.DropDownStyle = ComboBoxStyle.DropDownList;
            pcnames.Location = new Point(4, 5);
            pcnames.Width = 85;
            pcnames.Items.Add("Target PC");
            foreach (string item in machs)
                pcnames.Items.Add(item);
            pcnames.SelectedItem = pcnames.Items[0];
            pcnames.SelectedIndexChanged += new EventHandler(SetChoice);
            draggables.Controls.Add(pcnames);

            Label drag1 = new Label
            {
                Text = "Drag From Here:",
                Size = new Size(100, 20),
                Location = new Point(2, 10 + 20 * pcnames.Items.Count)
            };
            draggables.Controls.Add(drag1);

            choice.Location = new Point(4, drag1.Location.Y + 20);
            choice.Size = new Size(pcnames.Width, pcnames.Height);
            choice.KeyPress += new KeyPressEventHandler(BlockingInput);
            choice.MouseDown += new MouseEventHandler(TextMouseDown);
            draggables.Controls.Add(choice);

            
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

            graph = new Panel
            {
                Location = new Point(5, 470),
                Size = new Size(790, 230),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                AllowDrop = true
            };
            Controls.Add(graph);

            draggables = new Panel
            {
                Location = new Point(800, 470),
                Size = new Size(95, 150),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            Controls.Add(draggables);

            FillGraphFuncs();
            SetDraggables();

            InitializeComponent();
        }
    }
}
