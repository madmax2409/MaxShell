using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace server
{
    class Client
    {
        public static Dictionary<Socket, string[]> clients = new Dictionary<Socket, string[]>();
        public static Queue<Client> clientqueue = new Queue<Client>();
        private readonly Socket my;
        private readonly string nick;
        private readonly string mach;
        public Client(Socket cl, string nickname, string machname)
        {
            my = cl;
            nick = nickname;
            mach = machname;
            string[] st = { nick, mach, cl.RemoteEndPoint.ToString() };
            clients.Add(cl, st);
        }
        public Socket GetSocket()
        {
            return my;
        }
        public string GetNick()
        {
            return nick;
        }
        public string GetMach()
        {
            return mach;
        }
        public static void Disconnect(Socket dis)
        {
            clients.Remove(dis);
            dis.Close();
        }
        public static void CheckAndAdd(Socket cl, string mach, string nick)
        {
            bool flag = true;
            foreach (KeyValuePair<Socket, string[]> pair in clients)
            {
                if (pair.Value[1] == mach) //check if the client was registered in the session 
                {
                    flag = false;
                    break;
                }
            }
            if (flag || clients.Count == 0) //and adds only if no identical client was detected
                clientqueue.Enqueue(new Client(cl, nick, mach));

            Console.WriteLine("Added a new client: nick: {0}, mach: {1}", nick, mach);
        }
    }
}
