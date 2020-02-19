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
    public partial class Form3 : Form
    {
        private static TextBox entry = new TextBox();
        public static string nickname;
        public void SendNick(object sender, EventArgs e)
        {
            nickname = entry.Text;
            Close();
        }
        public Form3()
        {
            
            entry.Font = new Font("Comic Sans", 10);
            entry.Location = new Point(10, 10);
            Controls.Add(entry);

            Button choose = new Button();
            choose.Text = "ok";
            choose.Font = new Font("Comic Sans", 10);
            choose.Location = new Point(70, 40);
            choose.Click += new EventHandler(SendNick);
            Controls.Add(choose);

            InitializeComponent();
        }
    }
}
