using System;
using System.Drawing;
using System.Windows.Forms;

namespace terminal_graphics
{
    public partial class Login_Window : Form
    {
        private static TextBox entry = new TextBox();
        public static string nickname;
        public static bool assure = false;
        private const int CP_DISABLE_CLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle = cp.ClassStyle | CP_DISABLE_CLOSE_BUTTON;
                return cp;
            }
        }
        public void SendNick(object sender, EventArgs e)
        {
            nickname = entry.Text;
            Close();
        }
        public void SendNick_2(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
                if (entry.Text != "")
                {
                    nickname = entry.Text;
                    assure = true;
                    Close();
                }
        }

        public Login_Window()
        {
            KeyPreview = true;
            KeyDown += new KeyEventHandler(SendNick_2);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;


            entry.Font = new Font("Comic Sans", 10);
            entry.Size = new Size(280, 50);
            entry.Location = new Point(10, 10);
            Controls.Add(entry);
            

            Button choose = new Button();
            choose.Text = "ok";
            choose.Font = new Font("Comic Sans", 10);
            choose.Location = new Point(110, 40);
            choose.Click += new EventHandler(SendNick);
            Controls.Add(choose);

            InitializeComponent();
        }


    }
}
