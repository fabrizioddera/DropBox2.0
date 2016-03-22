using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace client.src
{
    class socket_client
    {
        internal static Socket Connect()
        {
            Socket senderSock;

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
            IPHostEntry ipHost = Dns.GetHostEntry("");
            // Gets first IP address associated with a localhost 
            IPAddress ipAddr = ipHost.AddressList[1];
            //IPAddress ipAddr = IPAddress.Parse("172.20.95.232");

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


            return senderSock;
        }


        internal static string Send(Socket senderSock2, string theMessageToSend)
        {

            
            var json = CreateJson.GetDirectory(new DirectoryInfo(@"C:\Users\Pc\Desktop\CLIENT"));
            string MD5 = CreateMD5.GetMD5HashData("value");

            // Sends data to a connected Socket. 
            //int bytesSend = myFiles.sendFile(value, senderSock2);


            string response = null;

            // Sending message 
            //string theMessageToSend = "";

            if (theMessageToSend.Contains("QUIT"))
            {
                Protocol.request("QUIT\r\n");                
            }
            else
            {
                response = Protocol.sendProtocol(theMessageToSend, senderSock2);
                ReceiveDataFromServer(senderSock2);            
            }

            return response;

        }

        internal static void ReceiveDataFromServer(Socket senderSock2)
        {

            // Receiving byte array  
            byte[] bytes = new byte[1024];

            // Receives data from a bound Socket. 
            int bytesRec = senderSock2.Receive(bytes);

            // Converts byte array to string 
            String theMessageToReceive = Encoding.Unicode.GetString(bytes, 0, bytesRec);

            // Continues to read the data till data isn't available 
            while (senderSock2.Available > 0)
            {
                bytesRec = senderSock2.Receive(bytes);
                theMessageToReceive += Encoding.Unicode.GetString(bytes, 0, bytesRec);
            }

            //tbReceivedMsg.Text +=
                Protocol.reciveProtocol(theMessageToReceive, senderSock2);


        }

        internal static void Disconnect(Socket senderSock2)
        {

            // Disables sends and receives on a Socket. 
            senderSock2.Shutdown(SocketShutdown.Both);

            //Closes the Socket connection and releases all resources 
            senderSock2.Close();


        }

    }
}
