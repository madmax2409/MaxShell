using System;
using System.Collections.Generic;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace server
{
    class WmiFuncs
    {
        private static int counter = 1;
        public static ManagementScope ms = new ManagementScope();
        public static Dictionary<string, string> paths = new Dictionary<string, string>();

        public static void SetPaths()
        {
            string data = ShowFolders();
            string[] datas = data.Split(new char[] { '\n' });
            foreach (string item in datas)
                try { 
                    paths.Add(item, ("\\\\" + Environment.MachineName + "\\" + item).Replace(':', '$')); 
                    }
                catch { }
                
        }
        public static void RemoteConnectTheScope(string target)
        {
            ConnectionOptions co = new ConnectionOptions();
            co.Username = "maxim";
            co.Password = "Barmaley2409";
            co.Impersonation = ImpersonationLevel.Impersonate;
            co.EnablePrivileges = true;
            Console.WriteLine("target: " + target);
            ms = new ManagementScope("\\\\" + target + "\\root\\cimv2", co);
        }

        public static void TryCon(string target)
        {
            if (target != Environment.MachineName)
                RemoteConnectTheScope(target);
            else
                ms = new ManagementScope("\\\\" + target + "\\root\\cimv2");
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
        public static string FreeSpace(string target)
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

        public static string ShowProcess(string target)
        {
            string outpt = "";
            try
            {
                SelectQuery oq = new SelectQuery("SELECT * FROM Win32_Process");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(ms,oq);

                foreach (ManagementObject queryObj in searcher.Get())
                    outpt += "Process: " + queryObj["Caption"] + "\n";
                return outpt;
            }
            catch (ManagementException e)
            {
                return "An error occurred while querying for WMI data: " + e.Message;
            }
        }

        public static string KillProcess(string target, string targetprocess)
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

        public static string RemoteProcess(string target, string procname)
        {
            object[] proc = new[] { procname };
            ManagementClass wmiProcess = new ManagementClass(ms, new ManagementPath("Win32_Process"), new ObjectGetOptions());
            wmiProcess.InvokeMethod("Create", proc);
            return "Process created";
        }

        public static string ShareFolder(string target, string sharefolder)
        {
            int counter = 1;
            string rightpath = "";
            if (!Directory.Exists(sharefolder))
            {
                rightpath = sharefolder.Replace(':', '$');
                Console.WriteLine("Created");
                Directory.CreateDirectory("\\\\" + target + "\\" + rightpath);
                
            }
            try
            {
                ManagementClass managementClass = new ManagementClass(ms, new ManagementPath("Win32_Share"), new ObjectGetOptions());
                ManagementBaseObject inParams = managementClass.GetMethodParameters("Create");
                ManagementBaseObject outParams;
                inParams["Description"] = "SharedTestApplication";
                inParams["Name"] = rightpath.Substring(3);
                inParams["Path"] = sharefolder;
                inParams["Type"] = 0x0;
                outParams = managementClass.InvokeMethod("Create", inParams, null);
                Console.WriteLine("rightpath: " + rightpath.Substring(3) + " sharefolder: " + sharefolder);
                if ((uint)outParams.Properties["ReturnValue"].Value != 0)
                    return "error no: " + (uint)outParams.Properties["ReturnValue"].Value;
                else
                {
                    ShowFolders();
                    paths.Add(sharefolder, rightpath);
                    return "folder successfully set as shared with the net-name " + inParams["Name"];
                }
            }
            catch (Exception ex)
            {
                counter++;
                return ShareFolder(target, sharefolder);
            }
        }

        public static string ListFiles(string target, string path)
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
                Console.WriteLine("rightpath: " + rightpath);
                foreach (KeyValuePair<string, string> pair in paths)
                    if (rightpath == pair.Key)
                    {
                        str = Directory.GetFiles(pair.Value);
                        break;
                    }
                if (str != null)
                    for (int i = 0; i < str.Length; i++)
                        output += str[i] + "\n";
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

            string newdir = @"\\" + source + @"\C$\dump_folders\dump_folder No " + counter + " from " + target; //copyfile from DESKTOP-21F9ULD where dir=C:\testfolder2\uuu.txt
            Directory.CreateDirectory(newdir);
            Console.WriteLine("Created dir on source");
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
    }
}
