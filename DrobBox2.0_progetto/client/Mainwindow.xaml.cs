using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.DirectoryServices;
using System.Xml;
using System.IO;

namespace DrobBox2._0_progetto
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SocketPermission permission;
        Socket sListener;
        IPEndPoint ipEndPoint;
        Socket handler;

        string rd;
        byte[] b1;
        string v;
        int m;
        TcpListener list;
        Int32 port = 5050;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (user.Text == " " && pwd.Text == " ")
            {
                tbStatus.Text = "COMPLIMENTI SEI CONNESSO!";

                String path = @"C:\Users\Pc\Desktop\Nuova cartella";

                if (System.IO.Directory.Exists(path) == false)
                {
                    // Try to create the directory.
                    DirectoryInfo di = Directory.CreateDirectory(path);
                }



                //TcpListener list = new TcpListener(localAddr, port);
                TcpListener list = new TcpListener(port);
                list.Start();
                TcpClient client1 = list.AcceptTcpClient();
                tbStatus2.Text = "Client trying to connect";
                StreamReader sr = new StreamReader(client1.GetStream());
                rd = sr.ReadLine();
                v = rd.Substring(rd.LastIndexOf('.') + 1);
                m = int.Parse(v);
                list.Stop();
                client1.Close();


                //TcpListener list = new TcpListener(localAddr,port1);
                list = new TcpListener(5051);
                list.Start();
                TcpClient client = list.AcceptTcpClient();
                Stream s = client.GetStream();
                b1 = new byte[28];
                s.Read(b1, 0, b1.Length);
                File.WriteAllBytes(path + "\\" + rd.Substring(0, rd.LastIndexOf('.')), b1);
                list.Stop();
                client.Close();
                tbStatus2.Text = "File Received......";
            }
            else
            {
                tbStatus.Text = "ERRORE! RIFARE IL LOGIN";
            }


        }


    }
}
