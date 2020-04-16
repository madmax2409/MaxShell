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
        public static IPAddress ipAddr;
        public static void PreSock()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName()); //get the local IP to create the endpoint
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
                if (ipHostInfo.AddressList[i].ToString().StartsWith("192"))
                    ipAddr = ipHostInfo.AddressList[i];

            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11001); //create the socket and wait for a message
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
            handler = listener.Accept(); //after recieveing connectino details, we wait for approval to sign in (the server has successfuly set a passowrd)
            data = "";
            while (data != "approved")
            {
                byteslen = handler.Receive(bytes);
                data = Encoding.Unicode.GetString(bytes, 0, byteslen);
            }
        }
    }
}
