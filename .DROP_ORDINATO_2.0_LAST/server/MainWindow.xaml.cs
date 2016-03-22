using server.src;
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

namespace server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Socket sListener;

        private TextBox tbAux = new TextBox();

        public MainWindow()
        {
            InitializeComponent();
            tbAux.SelectionChanged += tbAux_SelectionChanged;

            string path = @"C:\Users\Pc\Desktop\SERVER";
            if (System.IO.Directory.Exists(path) == false)
            {
                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(path);

            }

            try
            {
                sListener = socket_server.Start();
                tbStatus.Text = "Server started.";

                socket_server.Listen(sListener);
                tbStatus.Text += "\nServer is now listening on " + sListener.LocalEndPoint.ToString();
            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }


        }

        private void tbAux_SelectionChanged(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                tbMsgReceived.Text = tbAux.Text;
            }
            );
        }

        //private void Start_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        sListener = socket_server.Start();
        //        tbStatus.Text = "Server started.";
        //    }
        //    catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        //}

        //private void Listen_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        socket_server.Listen(sListener);
        //        tbStatus.Text = "Server is now listening on " + sListener.LocalEndPoint.ToString();
        //    }
        //    catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        //}

        //private void Send_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        socket_server.Send(tbMsgToSend.Text, handler);
        //    }
        //    catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        //}

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                socket_server.Close(sListener);
            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }

        private void db_Click(object sender, RoutedEventArgs e)
        {
            string path = @"C:\Users\Pc\Desktop\CLIENT\a.txt";
            string fileData = File.ReadAllText(path);

            FileInfo fi = new FileInfo(path);
            myDatabase db = new myDatabase();
            
            db.TryCreateTable(tbMsgToSend);

            string a = fi.LastAccessTime + " " + fi.LastWriteTime;
            //string[] input = a.Split(' ');


            int id = 1;
            string name = fi.Name;
            DateTime date = fi.LastWriteTime;
            int bytes = int.Parse(fi.Length+"");
            string md5 = CreateMD5.GetMD5HashData(fileData);
            string path2 = fi.FullName;


            db.AddFile(id, name, date, bytes, md5, path2);

            tbMsgToSend.Text += "\nID : " + id + "\nNAME : " + name + "\nDATE : " + date + "\nBYTE : " + bytes + "\nMD5 : " + md5 + "\nPATH : " + path2;            

        }
    }
}
