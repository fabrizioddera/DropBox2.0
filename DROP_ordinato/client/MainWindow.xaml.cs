﻿using Newtonsoft.Json.Linq;
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
        // Receiving byte array  
        byte[] bytes = new byte[1024];
        Socket senderSock;

        public MainWindow()
        {
            InitializeComponent();

            Send_Button.IsEnabled = false;
            Disconnect_Button.IsEnabled = false;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create one SocketPermission for socket access restrictions 
                SocketPermission permission = new SocketPermission(
                    NetworkAccess.Connect,    // Connection permission 
                    TransportType.Tcp,        // Defines transport types 
                    "",                       // Gets the IP addresses 
                    SocketPermission.AllPorts // All ports 
                    );

                // Ensures the code to have permission to access a Socket 
                permission.Demand();

                // Resolves a host name to an IPHostEntry instance            
                //IPHostEntry ipHost = Dns.GetHostEntry("Pc-TOSH");
                // Gets first IP address associated with a localhost 
                //IPAddress ipAddr = ipHost.AddressList[1];
                IPAddress ipAddr = IPAddress.Parse("172.20.95.232");

                // Creates a network endpoint 
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 4510);

                // Create one Socket object to setup Tcp connection 
                senderSock = new Socket(
                    ipAddr.AddressFamily,// Specifies the addressing scheme 
                    SocketType.Stream,   // The type of socket  
                    ProtocolType.Tcp     // Specifies the protocols  
                    );

                senderSock.NoDelay = false;   // Using the Nagle algorithm 

                // Establishes a connection to a remote host 
                senderSock.Connect(ipEndPoint);
                tbStatus.Text = "Socket connected to " + senderSock.RemoteEndPoint.ToString();

                Connect_Button.IsEnabled = false;
                Send_Button.IsEnabled = true;
            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }

        }

        JToken GetDirectory(DirectoryInfo directory)
        {
            return JToken.FromObject(new
            {
                directory = directory.EnumerateDirectories().ToDictionary(x => x.Name, x => GetDirectory(x)),
                file = directory.EnumerateFiles().Select(x => x.Name).ToList()
            });
        }


        private void Send_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                string value = @"C:\Users\Pc\Desktop\Nuova cartella";
                var json = GetDirectory(new DirectoryInfo(value));

                // Sending message 
                //<Client Quit> is the sign for end of data 
                byte[] msg = Encoding.Unicode.GetBytes(json + "<Client Quit>");

                // Sends data to a connected Socket. 
                int bytesSend = senderSock.Send(msg);

                ReceiveDataFromServer();

                Send_Button.IsEnabled = false;
                Disconnect_Button.IsEnabled = true;
            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }

        private void ReceiveDataFromServer()
        {
            try
            {
                // Receives data from a bound Socket. 
                int bytesRec = senderSock.Receive(bytes);

                // Converts byte array to string 
                String theMessageToReceive = Encoding.Unicode.GetString(bytes, 0, bytesRec);

                // Continues to read the data till data isn't available 
                while (senderSock.Available > 0)
                {
                    bytesRec = senderSock.Receive(bytes);
                    theMessageToReceive += Encoding.Unicode.GetString(bytes, 0, bytesRec);
                }

                tbReceivedMsg.Text = "The server reply: " + theMessageToReceive;
            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Disables sends and receives on a Socket. 
                senderSock.Shutdown(SocketShutdown.Both);

                //Closes the Socket connection and releases all resources 
                senderSock.Close();

                Disconnect_Button.IsEnabled = false;
            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }
    }
}