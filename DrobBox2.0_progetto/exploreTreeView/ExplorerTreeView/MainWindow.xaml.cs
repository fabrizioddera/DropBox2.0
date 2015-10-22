using System.Windows;
using ExplorerTreeView.Controls;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System;

namespace ExplorerTreeView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void explorer_ExplorerError1(object sender, ExplorerErrorEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }

        private void explorer_ExplorerError2(object sender, ExplorerErrorEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }

        byte[] bytes = new byte[1024];
        Socket senderSock;

        string n;
        byte[] b1;

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            explorer.SelectedPath = txtPath.Text;


            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                txtPath.Text = filename;

                FileInfo fi = new FileInfo(filename);
                n = fi.Name + "." + fi.Length;

                diff.Text = "Data:  " + fi.LastAccessTime + "\nByte:  " + fi.Length;

                TcpClient client = new TcpClient("Pc-TOSH", 5050);
                StreamWriter sw = new StreamWriter(client.GetStream());
                sw.WriteLine(n);
                sw.Flush();

                TcpClient client2 = new TcpClient("Pc-TOSH", 5051);
                Stream s = client2.GetStream();
                b1 = File.ReadAllBytes(filename);
                s.Write(b1, 0, b1.Length);
                client2.Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (trasferisci.IsChecked == true)
            {
                
            }
            else if (sostituisci.IsChecked == true)
            {

            }
            else if (elimina.IsChecked == true)
            {
                //FileInfo f = new FileInfo(filename);
                //f.Delete();
            }
            else
            {

            }
        }
    }
}
