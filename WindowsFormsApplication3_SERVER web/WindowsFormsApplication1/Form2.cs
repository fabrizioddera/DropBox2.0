using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace WindowsFormsApplication1
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        string rd;
        byte[] b1;
        string v;
        int m;
        TcpListener list;
        Int32 port = 5050;
        Int32 port1 = 5055;
//IPAddress localAddr = IPAddress.Parse("192.168.1.20");
        private void button1_Click(object sender, EventArgs e)
        {
            //TcpListener list = new TcpListener(localAddr, port);
            TcpListener list = new TcpListener(port);
            list.Start();
            TcpClient client1 = list.AcceptTcpClient();
            label4.Text = "Client trying to connect";
            StreamReader sr = new StreamReader(client1.GetStream());
            rd = sr.ReadLine();
            v = rd.Substring(rd.LastIndexOf('.') + 1);
            m = int.Parse(v);
            list.Stop();
            client1.Close();

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
                //TcpListener list = new TcpListener(localAddr,port1);
                list = new TcpListener(port);
                list.Start();
                TcpClient client = list.AcceptTcpClient();
                Stream s = client.GetStream();
                b1 = new byte[28];
                s.Read(b1, 0, b1.Length);
                File.WriteAllBytes(textBox1.Text + "\\" + rd.Substring(0, rd.LastIndexOf('.')), b1);
                list.Stop();
                client.Close();
                label4.Text = "File Received......";
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }


    }
}
