using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace server
{
    class WmiFuncs
    {
        private static int counter = 1;
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

        public static string ShowProcess(string target)
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

        public static string KillProcess(string targetmachine, string targetprocess)
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

            return "Could not kill the specified process, please recheck your parameters";
        }

        public static string RemoteProcess(string targetmachine, string procname)
        {
            ManagementClass cl = new ManagementClass("\\\\" + targetmachine + "\\root\\CIMV2:Win32_Process");
            object[] methodArgs = { procname, null, null, 0 };
            cl.InvokeMethod("Create", methodArgs);
            return "the process " + procname + " started successfully";
        }

        public static string ShareFolder(string target, string sharefolder)
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

        public static string ListFiles(string target, string path)
        {
            string p = "";
            string output = "";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("\\\\" + target + "\\root\\CIMV2", "SELECT * FROM Win32_Share");

            foreach (ManagementObject mo in searcher.Get())
            {
                Console.WriteLine("mo[path]: " + mo["path"].ToString() + " and path: " + path);
                if (mo["path"].ToString() != "" && path == mo["path"].ToString())
                    p = mo["path"].ToString();
            }

            if (p != "")
            {
                string rightpath = p.Substring(3);
                Console.WriteLine("rightpath: " + rightpath);
                string[] paths = Directory.GetFiles("\\\\" + target + "\\" + rightpath);
                for (int i = 0; i < paths.Length; i++)
                    output += paths[i] + "\n";
                return output;
            }
            else
                return "an error occurred";
        }

        public static string ShowFolders()
        {
            string output = "";
            Queue <Client> q = Client.clientqueue;
            //Console.WriteLine(q.Count);
            for(int i = 0; i < q.Count; i++)
            {
                Client temp = q.Dequeue();
                string mach = temp.GetMach();
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("\\\\" + mach + "\\root\\CIMV2", "SELECT * FROM Win32_Share");
                output += mach + "'s shared folders and drives: \n";
                foreach (ManagementObject mo in searcher.Get())
                    if (!mo["Name"].ToString().Contains("$"))
                        output += mo["Path"] + "\n";
                q.Enqueue(temp);
            }
            return output;
        }

        public static string CopyFile(string target, string srcpath)
        {
            string newdir = @"C:\dump_folders\dump_folder No " + counter + " from " + target; //copyfile from DESKTOP-21F9ULD where dir=C:\testfolder2\uuu.txt
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
            catch  //IOException e
            {
                return CopyFile(target, srcpath);
            }
        }
    }
}
