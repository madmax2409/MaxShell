using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;


namespace terminal_graphics
{
    public partial class Shell : Form
    {
        private Label direc = new Label();
        private TextBox command = new TextBox();
        private TextBox output = new TextBox();
        private Stack<string> older = new Stack<string>();
        private Stack<string> newer = new Stack<string>();

        private void BlockingInput(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void OutputProcedure(object sender, KeyEventArgs e)
        {
            TextBox cmd = (TextBox)sender;
            if (e.KeyData == Keys.Enter)
            {
                while (newer.Count > 0)
                    older.Push(newer.Pop());

                if (cmd.Text == "")
                    return;

                string command = cmd.Text;
                older.Push(command);
                string outpt = Program.Maintain(command);
                if (outpt == "")
                    return;
                char[] seperate = { '\n' };
                string[] dirs = outpt.Split(seperate);
                for (int i = 0; i < dirs.Length; i++)
                {
                    output.AppendText(dirs[i]);
                    output.AppendText(Environment.NewLine);
                }
                output.AppendText(Environment.NewLine);
                cmd.Text = "";
            }

            else if (e.KeyData == Keys.Up)
            {
                if (older.Count > 0)
                {
                    cmd.Text = older.Pop();
                    newer.Push(cmd.Text);
                    cmd.Select(cmd.Text.Length, 0);
                }
            }
            else if (e.KeyData == Keys.Down)
            {
                if (newer.Count > 0)
                {
                    cmd.Text = newer.Pop();
                    older.Push(cmd.Text);
                    cmd.Select(cmd.Text.Length, 0);
                }
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.ApplicationExitCall)
            {
                return;
            }

            switch (MessageBox.Show(this, "Are you sure you want to exit?", "Exiting", MessageBoxButtons.YesNo))
            {
                case DialogResult.No:
                    e.Cancel = true;
                    break;
                default:
                    if (Program.form2 != null)
                        Program.form2.Close();
                    break;
            }
        }
        public Shell()
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            Font f = new Font("comic sans", 10);
            direc.Font = f;
            direc.Location = new Point(0, 10);
            string dir = Directory.GetCurrentDirectory();
            char[] seperate = { '\\' };
            string[] dirs = dir.Split(seperate);
            if (dirs.Length > 3)
                dir = dirs[0] + @"\" + dirs[1] + @"\" + dirs[2] + @"\...\" + dirs[dirs.Length - 1] + ">";
            direc.Text = dir;
            SizeF size = direc.CreateGraphics().MeasureString(direc.Text, direc.Font);
            direc.Size = new Size((int)size.Width + 10, (int)size.Height + 2);
            Controls.Add(direc);

            command.Font = f;
            command.Location = new Point((int)size.Width + 11, 10);
            command.Size = new Size((this.Width - (int)size.Width - 15) * 5 + 30, 10);
            command.BorderStyle = BorderStyle.FixedSingle;
            command.KeyDown += new KeyEventHandler(OutputProcedure);
            Controls.Add(command);

            output.Font = f;
            output.Location = new Point(5, 40);
            output.Multiline = true;
            output.Size = new Size(790, 405);
            output.BorderStyle = BorderStyle.Fixed3D;
            output.ScrollBars = ScrollBars.Vertical;
            output.KeyPress += new KeyPressEventHandler(BlockingInput);
            Controls.Add(output);

            InitializeComponent();
        }
    }
}
