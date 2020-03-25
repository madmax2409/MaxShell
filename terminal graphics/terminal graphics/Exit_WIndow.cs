using System;
using System.Drawing;
using System.Windows.Forms;
using System.Timers;

namespace terminal_graphics
{
    public partial class Exit_WIndow : Form
    {
        private void Window_Loaded(object sender, EventArgs e)
        {
            System.Timers.Timer t = new System.Timers.Timer();
            t.Interval = 500;
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
            t.Start();
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            Close();
            Application.Exit();
        }

        public Exit_WIndow()
        {
            Text = "Exiting...";
            Load += new EventHandler(Window_Loaded);

            Label l = new Label();
            l.Font = new Font("Comic Sans", 10);
            l.Location = new Point(30, 10);
            l.Text = "Exiting...";
            Controls.Add(l);

            InitializeComponent();
        }
        
    }
}
