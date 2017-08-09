using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat
{
    public partial class Send_File : Form
    {
        public Send_File()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if(openFile.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text =  openFile.FileName.Split('\\').Last();
                textBox3.Text = openFile.FileName;
                FileInfo f = new FileInfo(openFile.FileName);
                long s1 = f.Length/1000;
                textBox2.Text = s1.ToString() + "Kb";
            }
        }
    }
}
