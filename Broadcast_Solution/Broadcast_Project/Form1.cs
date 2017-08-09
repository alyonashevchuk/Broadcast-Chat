using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Broadcast_Project
{
    public partial class Form1 : Form
    {
        UdpClient clietRecieve = null, clientSecnd = null;
        IPEndPoint ipendPoint1 = null, ipendPoint2=null; 

        private void Form1_Load(object sender, EventArgs e)
        {
            clietRecieve = new UdpClient(new IPEndPoint(Dns.Resolve(SystemInformation.ComputerName).AddressList[0], 47025));            
            clientSecnd = new UdpClient(new IPEndPoint(Dns.Resolve(SystemInformation.ComputerName).AddressList[0], 47023));
            timer1.Start();
            timer1.Interval = 1000;
            Thread t = new Thread(Func);
            t.IsBackground = true;
            t.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            byte[] b = Encoding.ASCII.GetBytes(SystemInformation.ComputerName);
            clientSecnd.Connect(IPAddress.Broadcast, 47025);
            clientSecnd.Send(b,b.Length);

        }

        void Func(object o)
        {
          //  UdpClient udp = o as UdpClient;
          
            while (true)
            {
                IPEndPoint p = new IPEndPoint(IPAddress.Any, 47025);
                byte[] m = clietRecieve.Receive(ref p);
                string message = Encoding.ASCII.GetString(m);
                if (!listBox1.Items.Contains(message))
                {
                   this.Invoke ((Action)(() =>
                    {
                        this.listBox1.Items.Add(message);
                    }));
                }
            }
        }
        public Form1()
        {
            InitializeComponent();
        }
    }
}
