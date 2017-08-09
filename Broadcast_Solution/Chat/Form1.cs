using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Chat
{
    
    public partial class Form1 : Form
    {
        private UdpClient clientSecnd;
        private UdpClient clietRecieve;
        string Nick = SystemInformation.ComputerName;
        ImageList imageList = new ImageList();
        public RichTextBox PrivateCurrentBox = null;
        public Form1()
        {
            InitializeComponent();
            KeyPreview = true;
            textBox2.Text = Nick;
            listView1.FullRowSelect = true;
            listView1.MultiSelect = false;
            Icon i1 = new Icon("M.ico");
            Icon i2 = new Icon("F.ico");
            imageList.Images.Add(i1);
            imageList.Images.Add(i2);
            listView1.LargeImageList = imageList;
            //     listView1.Items.Add(new ListViewItem(i));
            // create image list and fill it 

            //imageList.Images.Add("itemImageKey", i);
            // tell your ListView to use the new image list

            // add an item
            //var r =  listView1.Items.Add("Item with image");
            //r.ImageIndex = 0;

            //pictureBox1.Image = i;

            // and tell the item which image to use
            //         listViewItem.ImageKey = "itemImageKey";
            tabControl1.TabPages[0].Text = "Public chat";
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            clietRecieve = new UdpClient(new IPEndPoint(Dns.Resolve(SystemInformation.ComputerName).AddressList[0], 47035));
            clientSecnd = new UdpClient(new IPEndPoint(Dns.Resolve(SystemInformation.ComputerName).AddressList[0], 47033));
            //timer1.Start();
           // timer1.Interval = 1000;
            Thread t = new Thread(Func);
            t.IsBackground = true;
            t.Start();

            byte[] b = ObjectToByteArray(new MyMessage { TypeMessage = TypeOfMessage.Connect, IP_From = GetLocalIPAddress(), Name = Nick , ImageIndex = comboBox1.SelectedIndex});
            clientSecnd.Connect(IPAddress.Broadcast, 47035);
            clientSecnd.Send(b, b.Length);
            comboBox1.SelectedIndex = 0;

        }
        void Func(object o)
        {
            //  UdpClient udp = o as UdpClient;

            while (true)
            {
                IPEndPoint p = new IPEndPoint(IPAddress.Any, 47035);
                byte[] m = clietRecieve.Receive(ref p);
                MyMessage message = (MyMessage)ByteArrayToObject(m);
                try
                {
                    this.Invoke((Action)(() =>
                    {
                        switch (message.TypeMessage)
                        {
                            case TypeOfMessage.Connect:
                                var r = listView1.Items.Add(message.Name);
                                r.Tag = message.IP_From;
                                r.ImageIndex = message.ImageIndex;
                                byte[] b = ObjectToByteArray(new MyMessage { TypeMessage = TypeOfMessage.SendNameWhenConnectNewUser, IP_From = GetLocalIPAddress(), Name = Nick, ImageIndex = comboBox1.SelectedIndex });
                             //   clientSecnd.Connect(IPAddress.Broadcast, 47035);
                                clientSecnd.Send(b, b.Length);
                                break;
                            case TypeOfMessage.ChangeGender:
                                foreach (ListViewItem item in listView1.Items)
                                {
                                    if ((string)item.Tag == message.IP_From)
                                    {
                                        item.ImageIndex = message.ImageIndex;
                                    }
                                }
                                break;
                            case TypeOfMessage.Disconnect:
                                {
                                    foreach (ListViewItem item in listView1.Items)
                                    {
                                        if ((string)item.Tag == message.IP_From)
                                        {
                                            listView1.Items.Remove(item);
                                        }
                                    }
                                }
                                break;
                            case TypeOfMessage.SendNameWhenConnectNewUser:
                                foreach (ListViewItem item in listView1.Items)
                                {
                                   // bool f = false;
                                    if ((string)item.Tag == message.IP_From || item.Text == message.Name)
                                    {
                                        return;
                                    }
                               
                                }
                                var r2 = listView1.Items.Add(message.Name);
                                r2.Tag = message.IP_From;
                                r2.ImageIndex = message.ImageIndex;
                                break;
                            case TypeOfMessage.ChangeNickMessage:
                                foreach (ListViewItem item in listView1.Items)
                                {
                                    // bool f = false;
                                    if ((string)item.Tag == message.IP_From)
                                    {
                                        item.Text = message.Name;
                                     //   listView1.Update(); 
                                        return;
                                    }

                                }
                                break;
                            case TypeOfMessage.PublicMessage:
                                tabControl1.SelectedTab = tabControl1.TabPages[0];
                                richTextBox1.ReadOnly = false;
                                Clipboard.SetImage(imageList.Images[message.ImageIndex]);
                                richTextBox1.Paste();
                                richTextBox1.ReadOnly = true;
                                richTextBox1.AppendText(" " + message.Name + ": " + message.Message+"\n");
                                break;
                            case TypeOfMessage.PrivateMessage:
                                if (GetLocalIPAddress() == message.IP_To && GetLocalIPAddress() != message.IP_From)
                                {
                                    //  privateChatToolStripMenuItem_Click(this, new EventArgs());
                                   // if(!(message.IP_From == message.IP_To && message.IP_To == GetLocalIPAddress()))
                                    ShowOrCreateTab(message.Name, message.IP_From);
                                    PrivateCurrentBox.ReadOnly = false;
                                    Clipboard.SetImage(imageList.Images[message.ImageIndex]);

                                    PrivateCurrentBox.Paste();
                                    PrivateCurrentBox.ReadOnly = true;
                                    PrivateCurrentBox.AppendText(" " + message.Name + ": " + message.Message + "\n");
                                }
                              
                                break;
                            case TypeOfMessage.OpenPrivateTab:
                                if (GetLocalIPAddress() == message.IP_To && GetLocalIPAddress() != message.IP_From)
                                {
                                    //  privateChatToolStripMenuItem_Click(this, new EventArgs());
                                    // if(!(message.IP_From == message.IP_To && message.IP_To == GetLocalIPAddress()))
                                    ShowOrCreateTab(message.Name, message.IP_From);

                                }
                                break;
                        }
                    }));
                }
                catch (ObjectDisposedException)
                {
                    
                }
            }
        }

    

      

        //private void button2_Click(object sender, EventArgs e)
        //{
        //    Nick = textBox2.Text;
        //    byte[] b = Encoding.UTF8.GetBytes("!" + Nick);
        //    clientSecnd.Connect(IPAddress.Broadcast, 47035);
        //    clientSecnd.Send(b, b.Length);
        //}
        byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        object ByteArrayToObject (byte[] obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(obj))
            {
                object o = bf.Deserialize(ms);
                return o;
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            byte[] b = ObjectToByteArray(new MyMessage { TypeMessage = TypeOfMessage.ChangeGender, IP_From = GetLocalIPAddress(), ImageIndex = comboBox1.SelectedIndex });
            clientSecnd.Send(b,b.Length);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            byte[] b = ObjectToByteArray(new MyMessage { IP_From = GetLocalIPAddress(), TypeMessage = TypeOfMessage.Disconnect });
            clientSecnd.Send(b,b.Length);

        }



        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (textBox2.Text != string.Empty)
            {
                label1.Visible = false;
                Nick = textBox2.Text;
                byte[] b = ObjectToByteArray(new MyMessage { IP_From = GetLocalIPAddress(), TypeMessage = TypeOfMessage.ChangeNickMessage, Name = Nick });
                clientSecnd.Send(b, b.Length);
              
            }
            else
            {
                label1.Visible = true;
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                if (textBox1.Text != string.Empty)
                {
                    byte[] b = ObjectToByteArray(new MyMessage { TypeMessage = TypeOfMessage.PublicMessage, Message = textBox1.Text, ImageIndex = comboBox1.SelectedIndex, Name = Nick });
                    clientSecnd.Send(b, b.Length);
                    textBox1.Text = "";
                }
            }
        }
        private void NewTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (((TextBox)sender).Text != string.Empty)
                {
                    //MessageBox.Show("sdg");

                    byte[] b = ObjectToByteArray(new MyMessage { TypeMessage = TypeOfMessage.PrivateMessage, Message = ((TextBox)sender).Text, ImageIndex = comboBox1.SelectedIndex, Name = Nick, IP_To = (string)tabControl1.SelectedTab.Tag, IP_From = GetLocalIPAddress() });
                    clientSecnd.Send(b, b.Length);
                    PrivateCurrentBox.ReadOnly = false;
                    Clipboard.SetImage(imageList.Images[comboBox1.SelectedIndex]);

                    PrivateCurrentBox.Paste();
                    PrivateCurrentBox.ReadOnly = true;
                    PrivateCurrentBox.AppendText(" " + Nick + ": " + ((TextBox)sender).Text + "\n");
                    ((TextBox)sender).Text = "";
                }
            }
        }
        private void ShowOrCreateTab(string Name, string Sender_IP)
        {
            bool f = false;
            foreach (TabPage t in tabControl1.TabPages)
            {
                if (Sender_IP ==(string) t.Tag)
                {
                    f = true;
                    tabControl1.SelectedTab = t;
                    break;
                }


            }
            if (!f)
            {
                TabPage t = new TabPage(Name);
                RichTextBox r = new RichTextBox();
                r.ReadOnly = true;
                r.Location = richTextBox1.Location;
                r.Width = richTextBox1.Width;
                r.Height = richTextBox1.Height;
                PrivateCurrentBox = r;
                TextBox newTextBox = new TextBox();
                newTextBox.Location = textBox1.Location;
                newTextBox.Width = textBox1.Width;
                newTextBox.Height = textBox1.Height;
                newTextBox.KeyDown += NewTextBox_KeyDown;
                t.Controls.Add(r);
                t.Controls.Add(newTextBox);

                t.ContextMenuStrip = contextMenuStrip2;
                t.Tag = Sender_IP;
                tabControl1.TabPages.Add(t);
                tabControl1.SelectedTab = t;
                //add richtextbox and textbox
            }
        }
        private void privateChatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            if(listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select human");
            }
            else if ((string)listView1.SelectedItems[0].Tag == GetLocalIPAddress())
            {
                MessageBox.Show("You can't create chat with yourself");
            }
            else
            {
                ShowOrCreateTab(listView1.SelectedItems[0].Text, (string)listView1.SelectedItems[0].Tag);
                byte[] b = ObjectToByteArray(new MyMessage { TypeMessage = TypeOfMessage.OpenPrivateTab,Name = Nick, IP_To = (string)tabControl1.SelectedTab.Tag, IP_From = GetLocalIPAddress() });
                clientSecnd.Send(b, b.Length);
            }
        }

        private void closeCurrentTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab.Text != "Public chat")
                tabControl1.TabPages.Remove(tabControl1.SelectedTab);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.W)
            {
               // Выполнить нужное действие, например, открыть форму
               // MessageBox.Show("sdf");
                closeCurrentTabToolStripMenuItem_Click(this, e);
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var item in tabControl1.SelectedTab.Controls)
            {
                if(item as RichTextBox !=null)
                {
                    PrivateCurrentBox = item as RichTextBox;
                }
            } 
        }

        private void sendFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Send_File send = new Send_File();
           if(send.ShowDialog() == DialogResult.OK)
            {

            }
        }
    }
    public enum TypeOfMessage
    {
        Connect,
        Disconnect,
        SendNameWhenConnectNewUser,
        ChangeGender,
        PrivateMessage,
        PublicMessage,
        ChangeNickMessage,
        SendFileMessage,
        LoadFileMessage,
        OpenPrivateTab
    }
    [Serializable]
    public class MyMessage
    {
        public TypeOfMessage TypeMessage { get; set; }
        public string Name { get; set; }
        public string IP_From { get; set; }
        public string Message { get; set; }
        public string IP_To { get; set; }
        public int ImageIndex { get; set; }
        public byte[] File { get; set; }
    }
}
