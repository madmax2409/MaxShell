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
        private void CreateLabels(string data)
        {
            int x = 10, y = 25;
            foreach (string st in data.Split(new char[] { '\n' }))
            {
                Label lb = new Label();
                lb.Text = st;
                lb.Font = new Font(FontFamily.GenericSansSerif, 10);
                lb.Location = new Point(x, y);
                lb.Size = new Size(250, 200);
                y += 15;
                Controls.Add(lb);
            }
        }
        public ClientList(string data)
        {
            Font f = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);
            Label top = new Label();
            top.Location = new Point(5, 5);
            top.Size = new Size(350, 15);
            top.Text = "No    Nick       Mach       IP                       ";
            top.Font = f;
            Controls.Add(top);

            Label line = new Label();
            line.Location = new Point(5, 25);
            line.Text = "____________________________________________________";
            line.Size = new Size(350, 15);
            Controls.Add(line);

            //CreateLabels(data);
            InitializeComponent();
        }
    }
}
