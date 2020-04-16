using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace terminal_graphics
{
    public partial class ClientList : Form
    {
        private void CreateLabels(string data) //create lables for each of the connected pc's (including the server)
        {
            int x = 10, y = 50;
            int[] lengts = {20, 17, 0 };
            foreach (string st in data.Split(new char[] { '\n' })) //each line represents a client
            {
                Label lb = new Label();
                string[] para = st.Split(','); //divide each line by ',' to add padding inbetween for oraginized table
                for (int i = 0; i < para.Length; i++)
                {
                    if (i == 0)
                        para[i] = para[i].Insert(para[i].IndexOf('.') + 1, "     "); 

                    if (para[i].Length < lengts[i])
                    {
                        int dif = lengts[i] - para[i].Length;
                        string padding = " ";
                        for (int j = 0; j < dif; j++)
                            padding += " ";
                        para[i] += padding;
                    }
                }
                lb.Text = string.Join(" ", para); //create label and set location for next possible label
                lb.Font = new Font(FontFamily.GenericSansSerif, 10);
                lb.Location = new Point(x, y);
                lb.Size = new Size(400, 20);
                y += 25;
                Controls.Add(lb);
            }
        }
        public ClientList(string data)
        {
            BackColor = Color.LightSkyBlue;
            Font f = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);
            Label top = new Label
            {
                Location = new Point(5, 5),
                Size = new Size(350, 15),
                Text = "No    Nick       Mach                IP                       ",
                Font = f
            };
            Controls.Add(top);

            Label line = new Label
            {
                Location = new Point(5, 25),
                Text = "___________________________________________________",
                Size = new Size(350, 15)
            };
            Controls.Add(line);

            CreateLabels(data);
            InitializeComponent();
        }
    }
}
