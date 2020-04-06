using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Microsoft.VisualBasic.Devices;
using System.Device.Location;

namespace terminal_graphics
{
    public partial class SystemInfo : Form
    {
        private static Dictionary<string, string> dict = new Dictionary<string, string>();
        private static string[] shorts = { "Windwos Version", "Host Name", "Username", "CPU", "RAM", "ipv4", "ipv6", "MAC", "Subnet Mask", "area" };

        public static void BuildDict()
        {
            string[] methods = { 
                Environment.OSVersion.ToString(),
                Dns.GetHostName(),
                Environment.UserName,
                "CPU",
                GetRAM(),
                GetIpv4(),
                GetIpv6(),
                GetMacAddress(),
                GetSubnetMask(IPAddress.Parse(GetIpv4())),
                GetLocation()};

            for (int i = 0; i<shorts.Length; i++)
                dict.Add(shorts[i], methods[i]);
        }

        public static string GetIpv4()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
                if (ipHostInfo.AddressList[i].ToString().StartsWith("192"))
                    ipAddress = ipHostInfo.AddressList[i];
            return ipAddress.ToString();
        }

        public static string GetIpv6()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;
            foreach (IPAddress ad in addr)
                if (ad.AddressFamily == AddressFamily.InterNetworkV6)
                    return ad.ToString(); //ipv6

            return "no ipv6 address found";
        }

        public static string GetMacAddress()
        {
            return NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                .Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault();
        }

        public static string GetSubnetMask(IPAddress address)
        {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                        if (address.Equals(unicastIPAddressInformation.Address))
                            return unicastIPAddressInformation.IPv4Mask.ToString();
            return null;
        }

        public static string GetRAM()
        {
            ComputerInfo ci = new ComputerInfo();
            return ci.TotalPhysicalMemory.ToString();
        }
        private static string GetLocation()
        {
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();
            watcher.TryStart(false, TimeSpan.FromMilliseconds(1000));
            GeoCoordinate coord = watcher.Position.Location;
            if (coord.IsUnknown != true)
                return "Lat: " + coord.Latitude + " Long: " + coord.Longitude;
            else
                return "Unknown latitude and longitude";

        }

        public SystemInfo()
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            Panel sys = new Panel();
            sys.Location = new Point(20, 50);
            sys.Size = new Size(350, 380);
            sys.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(sys);

            Panel net = new Panel();
            net.Location = new Point(400, 50);
            net.Size = new Size(350, 380);
            net.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(net);

            Label s = new Label();
            s.Location = new Point(70, 15);
            s.Font = new Font("comic sans", 10);
            s.Text = "System Info";
            Controls.Add(s);

            Label n = new Label();
            n.Location = new Point(450, 15);
            n.Font = new Font("comic sans", 10);
            n.Text = "Network Info";
            Controls.Add(n);

            InitializeComponent();
        }
    }
}
