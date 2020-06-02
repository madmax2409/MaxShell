using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace terminal_graphics
{
    class GraphIntptr
    {
        private static TextBox choice = new TextBox();
        private static ComboBox pcnames = new ComboBox();
        public static Panel graph;
        public static Panel draggables;
        private static readonly Dictionary<string, string> funcnames = new Dictionary<string, string>();
        private static readonly string[] st = new string[] { "get cpu", "get ram", "get windows", "run", "kill", "free space", "path", "list processes", "create", "delete" };

        private static void TextMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) // Start the drag if it's the left mouse button.
                choice.DoDragDrop(choice.Text, DragDropEffects.Copy);
        }

        private static void Button_DragEnter(object sender, DragEventArgs e)  
        {
            e.Effect = DragDropEffects.Copy;
        }
        private static void OpenDraggedFile(object sender, DragEventArgs e) //get the path from the file name
        {
            var data = e.Data.GetData(DataFormats.FileDrop);
            if (data != null)
            {
                var filenames = data as string[];
                if (filenames.Length > 0)
                    Shell.PrintOut(filenames[0]); // and print
            }
        }
        private static void Button_DragDrop(object sender, DragEventArgs e)
        {
            string data = (string)e.Data.GetData(DataFormats.Text); //recieve text dragged from the textbox
            string command = ((Button)sender).Text;
            foreach (KeyValuePair<string, string> pair in funcnames)
                if (pair.Key == command)
                {
                    if (pair.Value == "create" || pair.Value == "delete" || pair.Value == "kill" || pair.Value == "run") //check if it's an action or an info request
                    {
                        if (Shell.DetectMach(data))
                            Program.Maintain(pair.Value, data);
                    }
                    else //and build the command accordingly
                    {
                        data = Program.Maintain(pair.Value + " on" + data);
                        Shell.PrintOut(data);
                    }

                    break;
                }
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
                if (text.Contains("Path"))
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

        public static void FillGraphFuncs()
        {
            Font f = new Font("Segue", 12);
            Label info = new Label
            {
                Font = f,
                Text = "To initiate a function, drag the parameter to the button space or simply click the button for a deafult command",
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
            BuildRow(new string[] { "CPU [PC Name]", "RAM [PC Name]", "Windows Ver. [PC Name]", "Run [PC Name]", "Kill [PC Name]" }, 70);
            BuildRow(new string[] { "Free Space [PC Name]", "Path [File]", "Processes [PC Name]", "Create [PC Name]", "Delete [PC Name]" }, 150);
        }

        private static void SetChoice(object sender, EventArgs e)
        {
            if (pcnames.SelectedItem.ToString() != "Target PC")
                choice.Text = pcnames.SelectedItem.ToString();
        }

        public static void SetDraggables()
        {
            string[] clients = Program.CallFunc("clients").Split(new char[] { '\n' });
            string[] machs = new string[clients.Length];
            for (int i = 0; i < clients.Length; i++)
                machs[i] = clients[i].Split(new char[] { ',' })[1];

            pcnames.DropDownStyle = ComboBoxStyle.DropDownList;
            pcnames.Location = new Point(4, 5);
            pcnames.Width = 85;
            pcnames.Items.Add("Target PC");

            Queue<string> names = new Queue<string>();
            foreach (string item in machs)
            {
                bool flag = false;
                for (int i = 0; i < names.Count; i++)
                {
                    string name = names.Dequeue();
                    if (item == name)
                    {
                        flag = true;
                        names.Enqueue(name);
                        break;
                    }
                }
                if (flag)
                    continue;

                pcnames.Items.Add(item);
                names.Enqueue(item);
            }
                
            pcnames.SelectedItem = pcnames.Items[0];
            pcnames.SelectedIndexChanged += new EventHandler(SetChoice);
            draggables.Controls.Add(pcnames);

            Label drag1 = new Label
            {
                Text = "Drag From Here:",
                Size = new Size(100, 20),
                Location = new Point(2, 180)
            };
            draggables.Controls.Add(drag1);

            choice.Location = new Point(4, 200);
            choice.Size = new Size(pcnames.Width, pcnames.Height);
            choice.KeyPress += new KeyPressEventHandler(Shell.BlockingInput);
            choice.MouseDown += new MouseEventHandler(TextMouseDown);
            draggables.Controls.Add(choice);
        }
    }
}
