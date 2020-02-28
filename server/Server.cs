using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Management;

namespace server
{
    class Server
    {
        private static int count = 1, counter = 1;
        private static Dictionary<string, Func<string[], string>> dict = new Dictionary<string, Func<string[], string>>();
        private static string[] funcs = { "getip", "freespace", "showproc", "disconnect", "killproc", "getdir", "startproc", "sharefolder", "listfiles", "write", "showfolders",
            "help", "copyfile" };

        public static void SetCommands()
        {
            Func<string[], string>[] methods = {
                targets => GetIp(),
                targets => FreeSpace(targets[1]),
                targets => ShowProcess(targets[1]),
                targets => Disconnect(),
                targets => KillProcess(targets[1], targets[2]),
                targets => Directory.GetCurrentDirectory(),
                targets => RemoteProcess(targets[1], targets[2]),
                targets =>ShareFolder(targets[1], targets[2]),
                targets =>ListFiles(targets[1], targets[2]),
                targets => Write(targets[1], targets[2], targets[3]),
                targets => ShowFolders(),
                targets => File.ReadAllText(Environment.CurrentDirectory + "\\info.txt"),
                targets =>CopyDir(targets[1], targets[2])};

            for (int i = 0; i < funcs.Length; i++)
                dict.Add(funcs[i], methods[i]);
        }

        public static string CommandOutput(string command)
        {
            char[] seperator = { ' ' };
            string[] param = command.Split(seperator);
            string output = "", cmd = param[0];
            bool flag = false;
            string[] pararms = new string[4];
            pararms[0] = cmd;

            switch (param.Length)
            {
                case 1:
                    pararms[1] = Environment.MachineName;
                    break;
                case 2:
                    pararms[1] = Environment.MachineName;
                    pararms[2] = param[1];
                    break;
                case 3:
                    pararms[1] = GetMach(param[1]);
                    pararms[2] = param[2];
                    break;
            }

            foreach (KeyValuePair<string, Func<string[], string>> pair in dict)
            {
                if (pair.Key == cmd)
                {
                    flag = true;
                    output = pair.Value(pararms);
                }
            }

            if (flag)
                return output + "stoprightnow";
            return "no such command as --> " + cmd + MostSimilar(cmd, funcs) + "stoprightnow";
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

        public static string Disconnect()
        {
            return "disconnect";
        }
        public static string GetIp()
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
                if (ipHostInfo.AddressList[i].ToString().StartsWith("192"))
                    ipAddress = ipHostInfo.AddressList[i];
            return ipAddress.ToString();
        }
        private static string FreeSpace(string target)
        {
            string outpt = "";
            var dskQuery = new SelectQuery("Win32_LogicalDisk", "DriveType=3"); // Define your query (what you want to return from WMI).
            var mgmtScope = new ManagementScope("\\\\" + target + "\\root\\cimv2"); // Define your scope (what system you want to connect to and WMI path).
            mgmtScope.Connect(); // Connect to WMI.
            var mgmtSrchr = new ManagementObjectSearcher(mgmtScope, dskQuery); //Define a searcher for the query.

            foreach (var disk in mgmtSrchr.Get()) //Call searcher’s Get method and loop through results.
            {
                var devId = disk.GetPropertyValue("DeviceID").ToString(); //Get the DeviceID of the current loop item and compare to the drive we want info on.
                if (!string.IsNullOrEmpty(devId))
                {
                    string freeWmi = disk.GetPropertyValue("FreeSpace").ToString(); //Get the value of the property we want (FreeSpace) and validate.
                    if (!string.IsNullOrEmpty(freeWmi))
                        outpt += "Free Space on " + devId + " Drive: " + freeWmi + " bytes\n";
                }
            }
            return outpt;
        }

        private static string ShowProcess(string target)
        {
            string outpt = "";
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("\\\\" + target + "\\root\\CIMV2", "SELECT * FROM Win32_Process");

                foreach (ManagementObject queryObj in searcher.Get())
                    outpt += "Process: " + queryObj["Caption"] + "\n";
                return outpt;
            }
            catch (ManagementException e)
            {
                return "An error occurred while querying for WMI data: " + e.Message;
            }
        }

        private static string KillProcess(string targetmachine, string targetprocess)
        {
            Console.WriteLine("target: " + targetmachine + " targetprocess: " + targetprocess);
            //Execute the query
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("\\\\" + targetmachine + "\\root\\CIMV2", "SELECT * FROM Win32_Process");

            foreach (ManagementObject ret in searcher.Get()) //loop through found processes and terminate
                if (ret["Name"].ToString().ToLower() == targetprocess)
                {
                    object[] obj = new object[] { 0 };
                    ret.InvokeMethod("Terminate", obj);
                    return "Termiated " + targetprocess + " on " + targetmachine + ", It can't harm us anymore";
                }

            return "Could not kill the specified process";
        }

        private static string RemoteProcess(string targetmachine, string procname)
        {
            ManagementClass cl = new ManagementClass("\\\\" + targetmachine + "\\root\\CIMV2:Win32_Process");
            object[] methodArgs = { procname, null, null, 0 };
            cl.InvokeMethod("Create", methodArgs);
            return "the process " + procname + " started successfully";
        }

        private static string ShareFolder(string target, string sharefolder)
        {
            if (!Directory.Exists(sharefolder))
            {
                string rightpath = sharefolder.Replace(':', '$');
                Directory.CreateDirectory("\\\\" + target + "\\" + rightpath);
            }
            try
            {
                Random rnd = new Random();
                ManagementClass managementClass = new ManagementClass("Win32_Share");
                ManagementBaseObject inParams = managementClass.GetMethodParameters("Create");
                ManagementBaseObject outParams;
                inParams["Description"] = "SharedTestApplication";
                inParams["Name"] = "sharefolder" + rnd.Next(10, 99);
                inParams["Path"] = sharefolder;
                inParams["Type"] = 0x0;
                outParams = managementClass.InvokeMethod("Create", inParams, null);

                if ((uint)outParams.Properties["ReturnValue"].Value != 0)
                    return "error no: " + (uint)outParams.Properties["ReturnValue"].Value;
                else
                    return "folder successfully set as shared with the net-name " + inParams["Name"];
            }
            catch (Exception ex)
            {
                return "an exception occured" + ex.Message;
            }
        }

        private static string Write(string target, string path, string text)
        {
            if (File.Exists(path))
            {
                FileStream fs = File.OpenWrite(path);
                byte[] bytes = Encoding.ASCII.GetBytes(text);
                fs.Write(bytes, 0, bytes.Length);
                return "successfull written to " + path + " the text:" + text;
            }
            else
                return "seems like the directory doesn't exist bucko";
        }

        private static string ListFiles(string target, string path)
        {
            string p = "";
            string output = "";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("\\\\" + target + "\\root\\CIMV2", "SELECT * FROM Win32_Share");

            foreach (ManagementObject mo in searcher.Get())
                if (path == mo["path"].ToString())
                    p = mo["path"].ToString();

            if (p != "")
            {
                string rightpath = p.Substring(3, p.Length - 3);
                string[] paths = Directory.GetFiles("\\\\" + target + "\\" + rightpath);
                for (int i = 0; i < paths.Length; i++)
                    output += paths[i] + "\n";
                return output;
            }
            else
                return "bruh you failed";
        }

        private static string ShowFolders()
        {
            string output = "";
            Queue<string> q = Program.machname;
            Console.WriteLine(q.Count);
            for (int i = 0; i < q.Count; i++)
            {
                string mach = q.Dequeue();
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("\\\\" + mach + "\\root\\CIMV2", "SELECT * FROM Win32_Share");

                output += mach + "'s shared folders and drives: \n";
                foreach (ManagementObject mo in searcher.Get())
                    if (!mo["Name"].ToString().Contains("$"))
                        output += mo["Path"] + "\n";

                q.Enqueue(mach);
            }
            return output;
        }

        private static string MostSimilar(string input, string[] commands)
        {
            int counter, len, max = 0;
            string sim = "";
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

            if (sim == "")
                return "";
            return ", did you mean " + sim + "?";
        }
        public static string CopyDir(string target, string srcpath)
        {
            string newdir = @"C:\dump_folders\dump_folder No " + counter + " from " + target;
            Directory.CreateDirectory(newdir);
            counter++;
            if (target == Environment.MachineName)
                File.Copy(srcpath, newdir + "\\" + Path.GetFileName(srcpath));
            else
                File.Copy(@"\\" + target + "\\" + srcpath, newdir + "\\" + Path.GetFileName(srcpath));
            return "copied the file";
        }
    }
}