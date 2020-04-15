using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;


namespace terminal_graphics
{
    public partial class SystemInfo : Form
    {
        private static Dictionary<string, string> dict = new Dictionary<string, string>();
        private static string[] shorts = { "Windwos Version", "Host Name", "Username", "CPU", "RAM", "ipv4", "ipv6", "MAC", "Subnet Mask" };
        public static Panel sys = new Panel();
        public static Panel net = new Panel();

        public static void BuildDict() //build the dictionary of the labels and outputs
        {
            string[] methods = { 
                PrintCom(Program.CallFunc("get windows on " + Environment.MachineName)),
                Dns.GetHostName(),
                Environment.UserName,
                PrintCom(Program.CallFunc("get cpu on " + Environment.MachineName)),
                PrintCom(Program.CallFunc("get ram on " + Environment.MachineName)),
                GetIpv4(),
                GetIpv6(),
                GetMacAddress(),
                GetSubnetMask(IPAddress.Parse(GetIpv4()))};

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
            string mac = NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                .Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault();
            int len = mac.Length, end = 0;
            for (int i = 2; i < len; i+=3)
            {
                mac = mac.Insert(i, "-");
                end = i;
            }
             mac = mac.Insert(end + 3, "-");

            return mac;
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

        public static void SetLabels() //build the labels
        {
            int counter = 1, x = 0, y = 0, mul = 0;
            Panel panel;
            foreach (KeyValuePair<string, string> pair in dict)
            {
                if (counter <= 5) //divide the list by system and network info
                {
                    panel = sys;
                    x = sys.Location.X;
                    y = sys.Location.Y;
                    mul = counter;
                }
                else
                {
                    panel = net;
                    x = net.Location.X;
                    y = net.Location.Y;
                    mul = counter - 5;
                }
                Label temp = new Label();
                temp.Font = new Font("comic sans", 11);
                temp.Size = new Size(350, 30);
                temp.Location = new Point(10, mul * 60 - 40);
                temp.Text = "\u2022 " + pair.Key + ": " + pair.Value; //build the label and add to the window
                panel.Controls.Add(temp);
                counter++;
            }
        }
        public static string PrintCom(string command) 
        {
            return command.Substring(0, command.IndexOf("stoprightnow"));
        }

        public SystemInfo()
        {
            BuildDict(); 

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.LightSkyBlue;

            Image comp = Image.FromFile(@"C:\MaxShell\pics\pc.png");
            Image nets = Image.FromFile(@"C:\MaxShell\pics\net.png");

            sys.Location = new Point(20, comp.Height + 20);
            sys.Size = new Size(350, 320);
            sys.BorderStyle = BorderStyle.FixedSingle;
            sys.BackColor = Color.White;
            Controls.Add(sys);

            net.Location = new Point(400, comp.Height + 20);
            net.Size = new Size(350, 320);
            net.BorderStyle = BorderStyle.FixedSingle;
            net.BackColor = Color.White;
            Controls.Add(net);

            Label s = new Label();
            s.Location = new Point(comp.Width + 20, comp.Height / 2 );
            s.Font = new Font("Segue", 18);
            s.Text = "System Info:";
            s.Size = new Size(200, 200);
            Controls.Add(s);

            Label n = new Label();
            n.Location = new Point(500, comp.Height / 2);
            n.Font = new Font("Segue", 18);
            n.Text = "Network Info:";
            n.Size = new Size(200, 200);
            Controls.Add(n);

            Label pcpic = new Label();
            pcpic.Location = new Point(105 - comp.Width, 15);
            pcpic.Size = new Size(comp.Width, comp.Height);
            pcpic.Image = comp;
            Controls.Add(pcpic);

            Label netpic = new Label();
            netpic.Location = new Point(490 - comp.Width, 15);
            netpic.Size = new Size(comp.Width, comp.Height);
            netpic.Image = nets;
            Controls.Add(netpic);

            SetLabels();

            InitializeComponent();
        }
    }
}
