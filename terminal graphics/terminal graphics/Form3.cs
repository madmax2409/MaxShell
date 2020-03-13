﻿using System;
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
        public static bool assure = false;
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
        public Form3()
        {
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(SendNick_2);

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
