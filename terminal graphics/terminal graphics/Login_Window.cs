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
        private static Label nick = new Label();
        private static Label pass = new Label();
        private static TextBox password = new TextBox();
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

            nick.Font = new Font("Comic Sans", 10);
            nick.Location = new Point(10, 10);
            nick.Size = new Size(75, 20);
            nick.Text = "nickname:";
            Controls.Add(nick);

            pass.Font = new Font("Comic Sans", 10);
            pass.Location = new Point(10, 40);
            pass.Size = new Size(75, 20);
            pass.Text = "password:";
            Controls.Add(pass);

            entry.Font = new Font("Comic Sans", 10);
            entry.Size = new Size(160, 50);
            entry.Location = new Point(85, 10);
            Controls.Add(entry);

            password.Font = new Font("Comic Sans", 10);
            password.Size = new Size(160, 80);
            password.Location = new Point(85, 40);
            Controls.Add(password);

            Button choose = new Button();
            choose.Text = "ok";
            choose.Font = new Font("Comic Sans", 10);
            choose.Location = new Point(85, 70);
            choose.Click += new EventHandler(SendNick);
            Controls.Add(choose);

            Button exit = new Button();
            exit.Text = "exit";
            exit.Font = new Font("Comic Sans", 10);
            exit.Location = new Point(170, 70);
            ///exit.Click += new EventHandler(SendNick);
            Controls.Add(exit);

            InitializeComponent();
        }


    }
}
