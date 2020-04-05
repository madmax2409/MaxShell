using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Windows.Forms;

namespace terminal_graphics
{
    class GetAddr
    {
        private static void ServMessages()
        {
            Process pr = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.FileName = "cmd.exex";
            startInfo.Arguments = "arp -a";
            pr.StartInfo = startInfo;
            pr.Start();
            string output = pr.StandardOutput.ReadToEnd();
            MessageBox.Show(output);

            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("192.168.1.214"), 11001);
            Socket sender2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender2.Connect(remoteEP);
        }
    }
}
