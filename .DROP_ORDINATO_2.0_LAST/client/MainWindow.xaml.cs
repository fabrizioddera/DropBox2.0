using client.src;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;


namespace client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket senderSock;

        public MainWindow()
        {
            InitializeComponent();
            //Watcher.Run();

            string path = @"C:\Users\Pc\Desktop\CLIENT";
            if (System.IO.Directory.Exists(path) == false)
            {
                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(path);

            }

            try
            {
                senderSock = socket_client.Connect();
                tbStatus.Text = "Socket connected to " + senderSock.RemoteEndPoint.ToString();

            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }

        }

        //private void Connect_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        senderSock = socket_client.Connect();
        //        tbStatus.Text = "Socket connected to " + senderSock.RemoteEndPoint.ToString();

        //    }
        //    catch (Exception exc) { MessageBox.Show(exc.ToString()); }

        //}

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string theMessageToSend = "SEND";
                tbReceivedMsg.Text = socket_client.Send(senderSock, theMessageToSend);
            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }

        //private void ReceiveDataFromServer()
        //{
        //    try
        //    {
        //        socket_client.ReceiveDataFromServer(senderSock);
        //    }
        //    catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        //}

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                socket_client.Disconnect(senderSock);
            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }

        private void Set_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string theMessageToSend = "+SET UPD";
                tbReceivedMsg.Text = socket_client.Send(senderSock, theMessageToSend);
            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }

        private void Get_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string theMessageToSend = "GET";
                tbReceivedMsg.Text = socket_client.Send(senderSock, theMessageToSend);
            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                tbReceivedMsg.Text += "cccccc   " + GetDirectorySize(@"C:\Users\Pc\Desktop\CLIENT");

                //string theMessageToSend = "QUIT";
                //tbReceivedMsg.Text = socket_client.Send(senderSock, theMessageToSend);
            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }

        private static long GetDirectorySize(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return di.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }
    }
}