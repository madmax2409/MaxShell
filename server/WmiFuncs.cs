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

        public static string ClientList() //returns specific data in on all connected clients in a list
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
        public static void AddPaths(string target) // a function to match paths of shared folders with full paths
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
        public static void RemoteConScope(string target) //build the connction scope with the user details
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

        public static void TryCon(string target) //builds the scope in accordance to the name of the computer
        {
            if (target != Environment.MachineName)
                RemoteConScope(target);
            else
                ms = new ManagementScope("\\\\" + target + "\\root\\cimv2");
        }
        private static string padding (string name)
        {
            if (name.Length < 35)
            {
                int len = 70 - name.Length;
                string pad = "";
                while (len > 0)
                {
                    pad += " ";
                    len--;
                }
                return pad;
            }
            return "";
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
            SelectQuery oq = new SelectQuery("SELECT * FROM Win32_Process");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(ms,oq);

            foreach (ManagementObject queryObj in searcher.Get()) //gets the list of all process objects
            {
                outpt += "Name: " + queryObj["Caption"] + padding(queryObj["Caption"].ToString()) + " PID: " + queryObj["ProcessID"] + "\n"; //retrieve the name property
            }
            return outpt;
        }

        public static string KillProcess(string targetprocess)
        {
            SelectQuery oq = new SelectQuery("SELECT * FROM Win32_Process");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(ms, oq);

            foreach (ManagementObject ret in searcher.Get()) //loop through found processes and terminate 
                if (ret["Name"].ToString().ToLower() == targetprocess) //we compare the names and find the one we want
                {
                    object[] obj = new object[] { 0 };
                    ret.InvokeMethod("Terminate", obj); 
                    return "Terminated " + targetprocess + ", It can't harm us anymore";
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
            if (!Directory.Exists("\\\\" + target + "\\" + rightpath)) // cannot share an unexisting direcory
                Directory.CreateDirectory("\\\\" + target + "\\" + rightpath);  //so we make sure it exists
            
            ManagementClass managementClass = new ManagementClass(ms, new ManagementPath("Win32_Share"), new ObjectGetOptions());
            ManagementBaseObject inParams = managementClass.GetMethodParameters("Create");
            ManagementBaseObject outParams;
            inParams["Description"] = "SharedTestApplication"; //set parameters for creation
            inParams["Name"] = rightpath.Substring(3); //name of folder
            inParams["Path"] = sharefolder; //full path of creation
            inParams["Type"] = 0x0; //drive
            outParams = managementClass.InvokeMethod("Create", inParams, null);
            if ((uint)outParams.Properties["ReturnValue"].Value != 0)
                return "error no: " + (uint)outParams.Properties["ReturnValue"].Value;
            else
            {
                paths.Add(sharefolder, rightpath); //save pah for future use
                return "folder successfully set as shared with the net-name " + inParams["Name"];
            }
        }

        public static string ListFiles(string path)
        {
            ManagementObject m = null;
            string output = "";
            SelectQuery oq = new SelectQuery("SELECT * FROM Win32_Share");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(ms, oq);

            foreach (ManagementObject mo in searcher.Get()) //find the path of the folder we want from the list of all shares
                if (mo["path"].ToString() != "" && path == mo["path"].ToString())
                {
                    m = mo; //save the path
                    break;
                }
                   
            if (m != null)
            {
                string[] str = null;
                string rightpath = m["Path"].ToString(); //get full path
                foreach (KeyValuePair<string, string> pair in paths)
                    if (rightpath == pair.Key) //search in list of saved folders
                    {
                        str = Directory.GetFiles(pair.Value); //and list the files in it
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
            for(int i = 0; i < q.Count; i++) //iterare on all clients and get their list of shared folders
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
            //add shared folders of the server
            ManagementObjectSearcher searcher2 = new ManagementObjectSearcher("\\\\" + Environment.MachineName + "\\root\\cimv2",
                "SELECT * FROM Win32_Share"); //win32_share contains the list of all shared instances (folders, drives...)
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
                if (src == pair.Key) //find the source of the command to know where to copy to
                {
                    source = pair.Value[1];
                    break;
                }

            if (target != Environment.MachineName)
                srcpath = srcpath.Replace(':', '$');
            //create a dump folder as a copy destination on the source machine
            string newdir = "\\\\" + source + "\\C$\\MaxShell\\dump_folders\\dump_folder No " + counter + " from " + target; //copyfile from DESKTOP-21F9ULD where dir=C:\testfolder2\uuu.txt
            Directory.CreateDirectory(newdir);
            ++counter;
            try
            { //copy the file according to the path and to the created dump folder
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
                        newdir = "\\\\" + target + "\\C$\\MaxShell\\dump_folders\\dump_folder No " + counter;
                    else
                        newdir = "C:\\MaxShell\\dump_folders\\dump_folder No " + counter;
                    Directory.CreateDirectory(newdir); //create a dump folder 
                    break;
                }
                catch { counter++; }
            }
            File.Create(newdir + "\\" + filename); //and create the file as written in the parameters
            return "created the file";
        }

        public static string DeleteFile(string target, string path)
        {//delete the file specifed by the path in the parameters
            path = path.Replace(':', '$');
            File.Delete("\\\\" + target + "\\" + path.Replace(':', '$')); 
            return "deleted the file";
        }

        public static string CPUName()
        {
            string output = "";
            ObjectQuery oq = new ObjectQuery("SELECT * FROM Win32_Processor"); //this class contains info about the processor
            ManagementObjectSearcher mos = new ManagementObjectSearcher(ms, oq);
            foreach (ManagementObject mo in mos.Get())
            {   // so we retrieve the processor's name
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
        {   //this class contains various info about the operating system and info
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
