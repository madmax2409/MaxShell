﻿using System;
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
        public static IPAddress ipAddr;
        public static void PreSock()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
                if (ipHostInfo.AddressList[i].ToString().StartsWith("192"))
                    ipAddr = ipHostInfo.AddressList[i];

            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11001);  //FreeTcpPort()           
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(3);
            Socket handler = listener.Accept();
            byte[] bytes = new byte[1024];
            int byteslen = handler.Receive(bytes);
            string data = Encoding.Unicode.GetString(bytes, 0, byteslen);
            string[] datas = data.Split(new char[] { ' ' });
            Program.ip = IPAddress.Parse(datas[0]);
            Program.port = int.Parse(datas[1]);
            handler = listener.Accept();
            data = "";
            while (data != "approved")
            {
                byteslen = handler.Receive(bytes);
                data = Encoding.Unicode.GetString(bytes, 0, byteslen);
            }

        }
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
