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
        public static Dictionary<Socket, string[]> clients = new Dictionary<Socket, string[]>();
        public static Queue<Client> q = new Queue<Client>();
        private Socket con;
        // private byte[] bytes;
        private string nick;
        private string mach;
        public Client(Socket cl, string nickname, string machname)
        {
            con = cl;
            nick = nickname;
            mach = machname;
            string[] st = { nick, mach };
            clients.Add(cl, st);
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
            clients.Remove(dis);
        }
        public static void CheckAndAdd(Socket cl, string mach, string nick)
        {
            bool flag = true;
            foreach(KeyValuePair<Socket, string[]> pair in clients)
            {
                if (pair.Value[1] == mach)
                {
                    flag = false;
                    break;
                }
            }
            if (flag || clients.Count == 0)
                q.Enqueue(new Client(cl, mach, nick));

            Console.WriteLine("Added a new client: mach: {0}, {1}",nick,mach);
        }
    }
}
