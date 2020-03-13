﻿using System;
using System.Collections.Generic;
using System.IO;


namespace server
{
    class Server
    {
        private static Dictionary<string, Func<string[], string>> dict = new Dictionary<string, Func<string[], string>>();
        private static string[] funcs = { "getip", "freespace", "showproc", "disconnect", "killproc", "getdir", "startproc", "sharefolder", "listfiles", "showfolders",
            "help", "copyfile" };
        private static string[] paramnames = { "name=", "dir=", "pc=" };

        public static void SetCommands()
        {
            Func<string[], string>[] methods = {
                targets => WmiFuncs.GetIp(),
                targets => WmiFuncs.FreeSpace(targets[1]),
                targets => WmiFuncs.ShowProcess(targets[1]),
                targets => Disconnect(),
                targets => WmiFuncs.KillProcess(targets[1], targets[2]),
                targets => Directory.GetCurrentDirectory(),
                targets => WmiFuncs.RemoteProcess(targets[1], targets[2]),
                targets =>WmiFuncs.ShareFolder(targets[1], targets[2]),
                targets =>WmiFuncs.ListFiles(targets[1], targets[2]),
                targets => WmiFuncs.ShowFolders(),
                targets => File.ReadAllText(Environment.CurrentDirectory + "\\info.txt"),
                targets =>WmiFuncs.CopyFile(targets[1], targets[2])};

            for (int i = 0; i < funcs.Length; i++)
                dict.Add(funcs[i], methods[i]);
        }

        public static string CommandOutput(string command)
        { 
            string output = "";
            bool flag = false;
            string[] pararms = Interpreter(command);
            if (pararms.Length >= 2)
            {
                foreach (KeyValuePair<string, Func<string[], string>> pair in dict)
                {
                    if (pair.Key == pararms[0])
                    {
                        flag = true;
                        output = pair.Value(pararms);
                    }
                }
            }

            if (flag)
                return output + "stoprightnow";
            return "no such command as --> " + pararms[0] + MostSimilar(pararms[0], funcs) + "stoprightnow";
        }
        
        private static string[] Interpreter(string command)
        {
            char[] sep = { ' ' }; 
            string[] cmd = command.Split(sep);
            Stack<string> param = new Stack<string>();
            bool cmdflag = false;
            string[] returned = new string[1];

            foreach (string possible in funcs)
            {
                if (cmd[0] == possible)
                {
                    param.Push(cmd[0]);
                    cmdflag = true;
                    break;
                }
            }

            if (cmdflag)
            {
                for (int i = 0; i < cmd.Length; i++)
                {
                    if (cmd[i] == "from" || cmd[i] == "on")
                    {
                        param.Push(cmd[i + 1]);
                        ++i;
                    }

                    else foreach (string para in paramnames)
                            if (cmd[i].Contains(para))
                                param.Push(cmd[i].Substring(cmd[i].IndexOf("=") + 1));
                }

                if (param.Count > 2)
                {
                    returned = new string[param.Count];
                    while (param.Count > 0)
                        returned[param.Count - 1] = param.Pop();
                }
                else
                {
                    returned = new string[param.Count+1];
                    param.Push(Environment.MachineName);
                    while (param.Count > 0)
                        returned[param.Count - 1] = param.Pop();
                }
            }
            else
            {
                returned = new string[2];
                returned[0] = cmd[0];
                returned[1] = Environment.MachineName;
            }
            return returned;
        }
        public static string GetMach(string target)
        {
            bool flag = false;
            int counter = 1;
            for (int i = 0; i < Program.machname.Count; i++)
            {
                string mach = Program.machname.Dequeue();
                Program.machname.Enqueue(mach);
                if (target == mach)
                {
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                for (int i = 0; i < Program.nickname.Count; i++)
                {
                    string nick = Program.nickname.Dequeue();
                    Program.nickname.Enqueue(nick);
                    if (target != nick)
                        counter++;
                    else
                        break;
                }

                while (counter > 0)
                {
                    target = Program.machname.Dequeue();
                    Program.machname.Enqueue(target);
                    counter--;
                }
            }
            return target;
        }

        private static string Disconnect()
        {
            return "disconnect";
        }
        private static string MostSimilar(string input, string[] commands)
        {
            int counter, len, max = 0;
            string sim = "";

            for (int i = 0; i < commands.Length; i++)
                if (commands[i].IndexOf(input) != -1 && (input.Length - sim.Length > 2 || input.Length - sim.Length < -2))
                    return ", did you mean " + commands[i] + "?";
                
            for (int i = 0; i < commands.Length; i++)
            {
                counter = 0;
                if (input.Length < commands[i].Length)
                    len = input.Length;
                else
                    len = commands[i].Length;

                for (int j = 0; j < len; j++)
                    if (input[j] == commands[i][j])
                        counter++;

                if (max < counter)
                {
                    max = counter;
                    sim = commands[i];
                }
            }

            if (input.Length - sim.Length > 2 || input.Length - sim.Length < -2)
                return "";

            if (sim == "")
                return "";
            return ", did you mean " + sim + "?";
        }
    }
}
