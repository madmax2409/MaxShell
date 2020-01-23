using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace server
{
    class Client
    {
        private static Queue<Socket> clients = new Queue<Socket>();
        private Socket con;
        private byte[] bytes;
        public Client(Socket cl)
        {
            con = cl;
            clients.Enqueue(con);
            bytes = new byte[4096];
        }
        public Socket GetSocket()
        {
            return con;
        }
        public void SetSocket(Socket sc)
        {
            this.con = sc;
        }
        public static void Disconnect(Socket dis)
        {
            bool flag = true;
            while (flag)
            {
                Socket sc = clients.Dequeue();
                if (sc == dis)
                {
                    flag = false;
                    break;
                }
                clients.Enqueue(sc);
            }
        }
    }
}
