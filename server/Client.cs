﻿using System;
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
        public static Queue<Client> clientqueue = new Queue<Client>();
        private Socket my;
        private Socket update; //notifications
        // private byte[] bytes;
        private string nick;
        private string mach;
        public Client(Socket cl, string nickname, string machname)
        {
            my = cl;
            nick = nickname;
            mach = machname;
            string[] st = { nick, mach };
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
                clientqueue.Enqueue(new Client(cl, nick, mach));

            Console.WriteLine("Added a new client: nick: {0}, mach: {1}",nick,mach);
        }
    }
}
