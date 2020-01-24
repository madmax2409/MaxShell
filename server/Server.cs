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
        public static string CommandOutput(string command)
        {
            char[] seperator = { ' ' };
            string[] param = command.Split(seperator);
            string cmd = param[0];
            string target = "";
            string parameter = "";
            if (param.Length == 2)
            {
                target = Environment.MachineName;
                parameter = param[1];
            }
            else if (param.Length > 2)
            {
                target = param[1];
                parameter = param[2];
            }
            switch (cmd)
            {
                case "getip":
                    string names = Dns.GetHostName();
                    return Dns.GetHostByName(names).AddressList[0].ToString();

                case "freespace":
                    return FreeSpace(target);

                case "showproc":
                    return ShowProcess(target);

                case "disconnect":
                    return "disconnect";

                case "killproc":
                    return KillProcess(target, parameter);

                case "getdir":
                    return Directory.GetCurrentDirectory();

                case "startproc":
                    return RemoteProcess(target, parameter);

                case "sharefolder":
                    return ShareFolder(target, parameter);

                case "showfolder":
                    return ShowFiles(target, parameter);

                case "write":
                    return Write(target, param[2], param[3]);

            }

            return "no such command as --> " + cmd;
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
                // Calling Win32_Share class to create a shared folder
                ManagementClass managementClass = new ManagementClass("Win32_Share");
                // Get the parameter for the Create Method for the folder
                ManagementBaseObject inParams = managementClass.GetMethodParameters("Create");
                ManagementBaseObject outParams;
                // Assigning the values to the parameters
                inParams["Description"] = "SharedTestApplication";
                inParams["Name"] = "sharedfolder";
                inParams["Path"] = sharefolder;
                inParams["Type"] = 0x0;
                // Finally Invoke the Create Method to do the process
                outParams = managementClass.InvokeMethod("Create", inParams, null);
                // Validation done here to check sharing is done or not
                if ((uint)outParams.Properties["ReturnValue"].Value != 0)
                    return "error no: " + (uint)outParams.Properties["ReturnValue"].Value;
                else
                    return "folder successfully set as shared";
            }
            catch (Exception ex)
            {
                return "an exception occured" + ex.Message;
            }
        }

        public static string Write(string target, string path, string text)
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

        public static string ShowFiles(string target, string path)
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
    }
}
    

