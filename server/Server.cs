using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace server
{
    class Server
    {
        private static readonly Dictionary<string, Func<string[], string>> dict = new Dictionary<string, Func<string[], string>>();
        private static readonly string[] funcs = { "get ip", "free space", "list processes", "disconnect", "kill", 
            "get directory", "run", "share", "list", "shared folders","help", "about", "copy", "create","delete", "clients", 
            "get cpu", "get ram", "get windows" }; 
        public static Queue<Socket> pass = new Queue<Socket>();

        public static void SetCommands()
        {
            Func<string[], string>[] methods = { //a dictionary of function names and their delegates
                targets => WmiFuncs.GetIp(),
                targets => WmiFuncs.FreeSpace(),
                targets => WmiFuncs.ShowProcess(),
                targets => Disconnect(),
                targets => WmiFuncs.KillProcess(targets[1]),
                targets => Directory.GetCurrentDirectory(),
                targets => WmiFuncs.RemoteProcess(targets[1]),
                targets => WmiFuncs.ShareFolder(targets[2], targets[1]),
                targets => WmiFuncs.ListFiles(targets[1]),
                targets => WmiFuncs.ShowFolders(),
                targets => File.ReadAllText(Environment.CurrentDirectory + "\\info.txt"),
                targets => File.ReadAllText(Environment.CurrentDirectory + "\\about.txt"),
                targets =>WmiFuncs.CopyFile(pass.Dequeue(), targets[2], targets[1]),
                targets => WmiFuncs.CreateFile(targets[2], targets[1]),
                targets => WmiFuncs.DeleteFile(targets[2], targets[1]),
                targets => WmiFuncs.ClientList(),
                targets => WmiFuncs.CPUName(),
                targets => WmiFuncs.TotalRAM(),
                targets => WmiFuncs.GetWinVer()};    

            for (int i = 0; i < funcs.Length; i++)
                dict.Add(funcs[i], methods[i]);
        }

        public static string CommandOutput(string command, Socket sc)
        {
            Console.WriteLine("message: " + command);
            string output = "";
            bool flag = false;
            string[] pararms = Interpreter(command); //interprets the command and returns a list of parameters
            if (pararms.Length >= 2)
            {
                foreach (KeyValuePair<string, Func<string[], string>> pair in dict)
                {
                    if (pair.Key == pararms[0]) //finds the right function
                    {
                        flag = true;
                        try
                        {
                            if (sc != null)
                                pass.Enqueue(sc); //enqueues the socket to check from which client was the message sent
                            WmiFuncs.TryCon(pararms[pararms.Length-1]); //tries to build the connection scope
                            output = pair.Value(pararms); //executes and gets the value of the function
                        }
                        catch 
                        {
                            output = "an error occurred, please check your parameters\n";// + e.ToString();
                        }
                    }
                }
            }
            if (flag) //if the command succceeded, the output is returned
                return output + "stoprightnow";
            else if (pararms[0] == "command failed")
                //if the command name is wrong, the most similar command is suggested to the client
                return "no such command as --> " + pararms[0] + MostSimilar(pararms[0]) + "stoprightnow";
            else
                //for any other reason, the output is as follows
                return "couldn't execute your command, please check your parametersstoprightnow";
        }
        
        private static string[] Interpreter(string command)
        {
            try
            {
                string[] keywords = { " from ", " on " };
                int end = 0, start = 0;
                string shortened, com, param = " ";
                char detect = ' ';
                bool flag = false, found = false;
                Stack<string> st = new Stack<string>();

                for (int i = 0; i < command.Length; i++) // remove double spaces
                    if (command[i] == ' ' && command[i + 1] == ' ')
                        command = command.Remove(i + 1, 1);

                foreach (string word in keywords) // search for pc name
                    if (command.IndexOf(word) != -1)
                    {
                        start = command.IndexOf(word) + word.Length;
                        shortened = command.Substring(start);
                        param = shortened.Substring(0);
                        flag = true;
                        break;
                    }

                if (flag)
                {
                    for (int i = 0; i < Client.clientqueue.Count; i++)
                    {
                        Client c = Client.clientqueue.Dequeue();
                        if (c.GetMach() == param || c.GetNick() == param)
                        {
                            st.Push(c.GetMach());
                            Client.clientqueue.Enqueue(c);
                            flag = false;
                            break;
                        }
                        Client.clientqueue.Enqueue(c);
                    }
                }
                else
                    st.Push(Environment.MachineName);

                if (command.IndexOf((char)34) != -1) //get additional parameters (in brackets)
                {
                    start = command.IndexOf((char)34);
                    detect = (char)34;
                }
                else if (command.IndexOf((char)39) != -1)
                {
                    start = command.IndexOf((char)39);
                    detect = (char)39;
                }
                if (detect != ' ')
                {
                    shortened = command.Substring(start + 1);
                    end = shortened.IndexOf(detect);
                    st.Push(shortened.Substring(0, end));
                    found = true;

                    command = command.Remove(start, end + 3);
                }

                foreach (string word in keywords)
                    if (command.IndexOf(word) != -1)
                        end = command.IndexOf(word) - 1;

                flag = false;
                start = 0;
                if (!found) //additional parameter without brackets
                {
                    if (end != 0)
                    {
                        for (int i = end; i >= 0; i--)
                        {
                            if (i == 0)
                                start = i;
                            else if (command[i] == ' ')
                            {
                                foreach (string cmd in funcs) //search for command name without any parameter
                                {
                                    if (cmd == command.Substring(0, end + 1))
                                    {
                                        flag = true;
                                        st.Push(cmd);
                                        Console.WriteLine(cmd);
                                        break;
                                    }
                                }
                                if (!flag) //get the parameter if the detected string didn't match any commmand
                                {
                                    start = i + 1;
                                    st.Push(command.Substring(start, end - start + 1));
                                    break;
                                }
                            }
                        }
                    }
                }
                try { param = command.Substring(0, end + 1); }
                catch { }

                if (!flag) //if no command was detected, searches and identifies a possibe command string
                {
                    if (start != 0)
                        com = command.Substring(0, start - 1); //check command name
                    else if (end != 0)
                        com = param;
                    else
                        com = command;

                    foreach (string cmd in funcs) //checks if such a command does exist
                        if (com == cmd)
                        {
                            st.Push(com); //saves it if it does
                            flag = true;
                            break;
                        }
                    if (!flag)
                        return new string[] { com }; //returns just the command to later inform the client about the mistake
                }

                string[] finite = new string[st.Count]; //organize and return a list from the stack
                int counter = 0;
                while (st.Count > 0)
                {
                    finite[counter] = st.Pop();
                    counter++;
                }
                return finite;
            }
            catch (Exception e) { Console.WriteLine(e.Message); return new string[] { "command failed" }; }
        }
        private static string Disconnect()
        {
            return "disconnect";
        }
        private static string MostSimilar(string input)
        {
            int counter, len, max = 0;
            string sim = "";

            for (int i = 0; i < funcs.Length; i++) //tries to check whether the client didn't use the full name of the command
                if (funcs[i].IndexOf(input) != -1 && (input.Length - sim.Length > 2 || input.Length - sim.Length < -2))
                    return ", did you mean " + funcs[i] + "?";
                
            for (int i = 0; i < funcs.Length; i++) //finds the most similar command by length
            {
                counter = 0;
                if (input.Length < funcs[i].Length)
                    len = input.Length;
                else
                    len = funcs[i].Length;

                for (int j = 0; j < len; j++)
                    if (input[j] == funcs[i][j])
                        counter++;

                if (max < counter)
                {
                    max = counter;
                    sim = funcs[i];
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
