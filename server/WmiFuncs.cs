using System;
using System.Collections.Generic;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;

namespace server
{
    class WmiFuncs
    {
        private static int counter = 1;
        public static ManagementScope ms = new ManagementScope();
        public static Dictionary<string, string> paths = new Dictionary<string, string>();

        public static string ClientList()
        {
            string output = "";
            int counter = 1;
            foreach (KeyValuePair<Socket, string[]> pair in Client.clients)
            {
                output += counter + ". " + pair.Value[0] + ", " + pair.Value[1] + ", " + 
                    pair.Value[2].Substring(0, pair.Value[2].IndexOf(':')) + "\n";
                counter++;
            }
            output += counter + ". " + "None, " + Environment.MachineName + ", " + GetIp() + " (sever)"; 
            return output;
        }
        public static void AddPaths(string target)
        {
            string data = ShowFolders();
            string[] datas = data.Split(new char[] { '\n' });
            for (int i = 0; i < datas.Length; i++)
            {
                try
                {
                    if (Regex.IsMatch(datas[i], @"^\w:\\[a-zA-Z0-9]*$") && !datas[i].Contains(target)) //C:\testfolder1
                        paths.Add(datas[i], ("\\\\" + target + "\\" + datas[i]).Replace(':', '$'));
                }
                catch { }
            }              
        }
        public static void RemoteConScope(string target)
        {
            ConnectionOptions co = new ConnectionOptions
            {
                Username = "maxim",
                Password = "Barmaley2409",
                Impersonation = ImpersonationLevel.Impersonate,
                EnablePrivileges = true
            };
            ms = new ManagementScope("\\\\" + target + "\\root\\cimv2", co);
        }

        public static void TryCon(string target)
        {
            if (target != Environment.MachineName)
                RemoteConScope(target);
            else
                ms = new ManagementScope("\\\\" + target + "\\root\\cimv2");
        }
    
        public static string GetIp()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
                if (ipHostInfo.AddressList[i].ToString().StartsWith("192"))
                    ipAddress = ipHostInfo.AddressList[i];
            return ipAddress.ToString();
        }
        public static string FreeSpace()
        {
            string outpt = "";
            SelectQuery dskQuery = new SelectQuery("Win32_LogicalDisk", "DriveType=3"); // Define your query (what you want to return from WMI).
            ManagementObjectSearcher mgmtSrchr = new ManagementObjectSearcher(ms, dskQuery); //Define a searcher for the query.

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

        public static string ShowProcess()
        {
            string outpt = "";
            try
            {
                SelectQuery oq = new SelectQuery("SELECT * FROM Win32_Process");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(ms,oq);

                foreach (ManagementObject queryObj in searcher.Get())
                    outpt += queryObj["Caption"] + "\n";
                return outpt;
            }
            catch (ManagementException e)
            {
                return "An error occurred while querying for WMI data: " + e.Message;
            }
        }

        public static string KillProcess(string targetprocess)
        {
            //Execute the query
            SelectQuery oq = new SelectQuery("SELECT * FROM Win32_Process");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(ms, oq);

            foreach (ManagementObject ret in searcher.Get()) //loop through found processes and terminate
                if (ret["Name"].ToString().ToLower() == targetprocess)
                {
                    object[] obj = new object[] { 0 };
                    ret.InvokeMethod("Terminate", obj);
                    return "Termiated " + targetprocess + ", It can't harm us anymore";
                }

            return "Could not kill the specified process, please recheck your parameters";
        }

        public static string RemoteProcess(string procname)
        {
            object[] proc = new[] { procname };
            ManagementClass wmiProcess = new ManagementClass(ms, new ManagementPath("Win32_Process"), new ObjectGetOptions());
            wmiProcess.InvokeMethod("Create", proc);
            return "Process created";
        }

        public static string ShareFolder(string target, string sharefolder)
        {
            string rightpath = sharefolder.Replace(':', '$');
            if (!Directory.Exists("\\\\" + target + "\\" + rightpath))
            {
                Directory.CreateDirectory("\\\\" + target + "\\" + rightpath);
            }
            ManagementClass managementClass = new ManagementClass(ms, new ManagementPath("Win32_Share"), new ObjectGetOptions());
            ManagementBaseObject inParams = managementClass.GetMethodParameters("Create");
            ManagementBaseObject outParams;
            inParams["Description"] = "SharedTestApplication";
            inParams["Name"] = rightpath.Substring(3);
            inParams["Path"] = sharefolder;
            inParams["Type"] = 0x0;
            outParams = managementClass.InvokeMethod("Create", inParams, null);
            if ((uint)outParams.Properties["ReturnValue"].Value != 0)
                return "error no: " + (uint)outParams.Properties["ReturnValue"].Value;
            else
            {
                paths.Add(sharefolder, rightpath);
                return "folder successfully set as shared with the net-name " + inParams["Name"];
            }
        }

        public static string ListFiles(string path)
        {
            ManagementObject m = null;
            string output = "";
            SelectQuery oq = new SelectQuery("SELECT * FROM Win32_Share");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(ms, oq);

            foreach (ManagementObject mo in searcher.Get())
                if (mo["path"].ToString() != "" && path == mo["path"].ToString())
                {
                    m = mo;
                    break;
                }
                   
            if (m != null)
            {
                string[] str = null;
                string rightpath = m["Path"].ToString();
                foreach (KeyValuePair<string, string> pair in paths)
                    if (rightpath == pair.Key)
                    {
                        str = Directory.GetFiles(pair.Value);
                        break;
                    }
                if (str != null)
                    for (int i = 0; i < str.Length; i++)
                        output += str[i].Substring(str[i].IndexOf(rightpath.Replace(':', '$')) + rightpath.Length + 1) + "\n";
                return output;
            }
            else
                return "an error occurred";
        }

        public static string ShowFolders()
        {
            string output = "";
            Queue <Client> q = Client.clientqueue;
            for(int i = 0; i < q.Count; i++)
            {
                Client temp = q.Dequeue();
                string mach = temp.GetMach();
                TryCon(mach);
                SelectQuery sq = new SelectQuery("SELECT * FROM Win32_Share");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(ms,sq);
                output += mach + "'s shared folders and drives: \n";
                foreach (ManagementObject mo in searcher.Get())
                {
                    if (!mo["Name"].ToString().Contains("$") && mo["Path"].ToString().Length > 3 && mo["Name"].ToString() != "Users")
                        output += mo["Path"] + "\n";
                }
                q.Enqueue(temp);
            }

            ManagementObjectSearcher searcher2 = new ManagementObjectSearcher("\\\\" + Environment.MachineName + "\\root\\cimv2", "SELECT * FROM Win32_Share");
            output +=  "\n" + Environment.MachineName + "'s shared folders and drives: \n";
            foreach (ManagementObject mo in searcher2.Get())
                if (!mo["Name"].ToString().Contains("$") && mo["Name"].ToString().Length > 3 && mo["Name"].ToString() != "Users")
                    output += mo["Path"] + "\n";

            return output;
        }

        public static string CopyFile(Socket src, string target, string srcpath)
        {
            string source = "";
            foreach (KeyValuePair<Socket, string[]> pair in Client.clients)
                if (src == pair.Key)
                {
                    source = pair.Value[1];
                    break;
                }

            if (target != Environment.MachineName)
                srcpath = srcpath.Replace(':', '$');

            string newdir = "\\\\" + source + "\\C$\\dump_folders\\dump_folder No " + counter + " from " + target; //copyfile from DESKTOP-21F9ULD where dir=C:\testfolder2\uuu.txt
            Directory.CreateDirectory(newdir);
            ++counter;
            try
            {
                if (target == Environment.MachineName)
                    File.Copy(srcpath, newdir + "\\" + Path.GetFileName(srcpath));
                else
                    File.Copy(@"\\" + target + "\\" + srcpath, newdir + "\\" + Path.GetFileName(srcpath));

                return "copied the file";
            }
            catch (IOException)
            {
                return CopyFile(src, target, srcpath);
            }
        }

        public static string CreateFile(string target, string filename)
        {
            string newdir;
            while (true)
            {
                try
                {
                    if (target != Environment.MachineName)
                        newdir = "\\\\" + target + "\\C$\\dump_folders\\dump_folder No " + counter;
                    else
                        newdir = "C:\\dump_folders\\dump_folder No " + counter;
                    Directory.CreateDirectory(newdir);
                    Console.WriteLine("Created the dir");
                    break;
                }
                catch { counter++; }
            }
            Console.WriteLine(newdir + "\\" + filename);
            File.Create(newdir + "\\" + filename);
            Console.WriteLine("bruh?");
            return "created " + filename + " on " + target;
        }

        public static string DeleteFile(string target, string path)
        {
            path = path.Replace(':', '$');
            File.Delete("\\\\" + target + "\\" + path.Replace(':', '$'));
            return "deleted " + path + " on " + target;
        }

        public static string CPUName()
        {
            string output = "";
            ObjectQuery oq = new ObjectQuery("SELECT * FROM Win32_Processor");
            ManagementObjectSearcher mos = new ManagementObjectSearcher(ms, oq);
            foreach (ManagementObject mo in mos.Get())
            {
                output = mo["Name"].ToString().Substring(0, mo["Name"].ToString().IndexOf('@')-1) + "\n";
            }
            return output;
        }

        public static string TotalRAM()
        {
            string output = "";
            ObjectQuery oq = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher mos = new ManagementObjectSearcher(ms, oq);
            foreach (ManagementObject mo in mos.Get())
            {
                output = mo["TotalVisibleMemorySize"].ToString();
            }
            return output;
        }

        public static string GetWinVer()
        {
            string output = "";
            ObjectQuery oq = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher mos = new ManagementObjectSearcher(ms, oq);
            foreach (ManagementObject mo in mos.Get())
            {
                output = mo["Caption"].ToString();
            }
            return output;
        }
    }
}
