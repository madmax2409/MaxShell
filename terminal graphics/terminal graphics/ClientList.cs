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
        private static TextBox tb = new TextBox();
        private static string AllignText(string data)
        {
            string newdata = " Nick               Mach               IP ";
            tb.AppendText(newdata + "\n" );
            tb.AppendText("bruh");
            //data = data.Replace(" , ", "                 ");
            string[] dirs = data.Split(new char[] { '\n' });
            for (int i = 0; i < dirs.Length; i++)
            {
                if (dirs[i] != "")
                {
                    tb.AppendText(dirs[i]);
                    MessageBox.Show(dirs[i]);
                    tb.AppendText(Environment.NewLine);
                }
            }
            return newdata;
        }

        private void CreateLabels(string data)
        {
            int x = 10, y = 10;
            foreach (string st in data.Split(new char[] { '\n' }))
            {
                MessageBox.Show(st);
                Label lb = new Label();
                lb.Text = st;
                lb.Font = new Font("comic sans", 10);
                lb.Location = new Point(x, y);
                lb.Size = new Size(250, 200);
                x += 10;
                Controls.Add(lb);
            }
        }
        public ClientList(string data)
        {
            CreateLabels(data);
            InitializeComponent();
        }
    }
}
