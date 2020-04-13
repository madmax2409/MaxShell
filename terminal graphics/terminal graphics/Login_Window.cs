﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Net.Sockets;

namespace terminal_graphics
{
    public partial class Login_Window : Form
    {
        private static TextBox entry = new TextBox();
        public static string nickname;
        public static string pas;
        public static bool assure = false;
        private const int CP_DISABLE_CLOSE_BUTTON = 0x200;
        private static Label nick = new Label();
        private static Label pass = new Label();
        private static TextBox password = new TextBox();
        private static Socket socket;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle = cp.ClassStyle | CP_DISABLE_CLOSE_BUTTON;
                return cp;
            }
        }
        public void Send(object sender, EventArgs e)
        {
            nickname = entry.Text;
            pas = password.Text;
            byte[] rec = new byte[4096];
            socket.Send(Encoding.Unicode.GetBytes(pas));
            int bytesrec = socket.Receive(rec);
            string data = Encoding.Unicode.GetString(rec, 0, bytesrec);
            if (data == "good to go")
            {
                Program.check = true;
                Close();
            }
            else
                MessageBox.Show("Wrong password, please try again");
        }
        public void Send_2(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
                if (entry.Text != "")
                {
                    nickname = entry.Text;
                    pas = password.Text;
                    byte[] rec = new byte[4096];
                    socket.Send(Encoding.Unicode.GetBytes(pas));
                    int bytesrec = socket.Receive(rec);
                    string data = Encoding.Unicode.GetString(rec, 0, bytesrec);
                    if (data == "good to go")
                    {
                        Program.check = true;
                        assure = true;
                        Close();
                    }
                    else
                        MessageBox.Show("Wrong password, please try again");
                }
        }
        public void Disconnect(object sender, EventArgs e)
        {
            socket.Close();
            Close();
        }
        public Login_Window(Socket s)
        {
            socket = s;
            Image img = Image.FromFile(@"C:\MaxShell\pics\logomax.png");

            Label logo = new Label();
            logo.Location = new Point(5, 2);
            logo.Size = new Size(250, 100);
            logo.Image = img;
            Controls.Add(logo);

            KeyPreview = true;
            KeyDown += new KeyEventHandler(Send_2);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            nick.Font = new Font("Segue", 10);
            nick.Location = new Point(10, 105);
            nick.Size = new Size(75, 20);
            nick.Text = "nickname:";
            Controls.Add(nick);

            pass.Font = new Font("Segue", 10);
            pass.Location = new Point(10, 135);
            pass.Size = new Size(75, 20);
            pass.Text = "password:";
            Controls.Add(pass);

            entry.Font = new Font("Segue", 10);
            entry.Size = new Size(160, 50);
            entry.Location = new Point(85, 105);
            Controls.Add(entry);

            password.Font = new Font("Segue", 10);
            password.Size = new Size(160, 80);
            password.Location = new Point(85, 135);
            Controls.Add(password);

            Button choose = new Button();
            choose.Text = "ok";
            choose.Font = new Font("Segue", 10);
            choose.Location = new Point(85, 165);
            choose.Click += new EventHandler(Send);
            Controls.Add(choose);

            Button exit = new Button();
            exit.Text = "exit";
            exit.Font = new Font("Segue", 10);
            exit.Location = new Point(170, 165);
            exit.Click += new EventHandler(Disconnect);
            Controls.Add(exit);

            InitializeComponent();
        }


    }
}
