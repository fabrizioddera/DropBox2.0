using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace server.src
{
    class socket_server
    {

        internal static Socket Start()
        {
            SocketPermission permission;
            Socket sListener;
            IPEndPoint ipEndPoint;

            // Creates one SocketPermission object for access restrictions
            permission = new SocketPermission(
            NetworkAccess.Accept,     // Allowed to accept connections 
            TransportType.Tcp,        // Defines transport types 
            "",                       // The IP addresses of local host 
            SocketPermission.AllPorts // Specifies all ports 
            );

            // Listening Socket object 
            sListener = null;

            // Ensures the code to have permission to access a Socket 
            permission.Demand();

            // Resolves a host name to an IPHostEntry instance 
            IPHostEntry ipHost = Dns.GetHostEntry("");
            // Gets first IP address associated with a localhost 
            IPAddress ipAddr = ipHost.AddressList[1];
            //IPAddress ipAddr = IPAddress.Parse("172.20.95.232");

            // Creates a network endpoint 
            ipEndPoint = new IPEndPoint(ipAddr, 4510);

            // Create one Socket object to listen the incoming connection 
            sListener = new Socket(
                ipAddr.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
                );

            // Associates a Socket with a local endpoint 
            sListener.Bind(ipEndPoint);

            return sListener;
        }

        internal static void Listen(Socket sListener2)
        {
            // Places a Socket in a listening state and specifies the maximum 
            // Length of the pending connections queue 
            sListener2.Listen(10);

            // Begins an asynchronous operation to accept an attempt 
            Callbacks c = new Callbacks();
            AsyncCallback aCallback = new AsyncCallback(c.AcceptCallback);
            sListener2.BeginAccept(aCallback, sListener2);

        }

        //internal static void Send(string str, Socket handler)
        //{

        //    // Prepare the reply message 
        //    byte[] byteData = Encoding.Unicode.GetBytes(str);

        //    // Sends data asynchronously to a connected Socket 
        //    handler.BeginSend(byteData, 0, byteData.Length, 0,
        //        new AsyncCallback(SendCallback), handler);

        //}

        internal static void SendCallback(IAsyncResult ar)
        {
            // A Socket which has sent the data to remote host 
            Socket handler = (Socket)ar.AsyncState;

            // The number of bytes sent to the Socket 
            int bytesSend = handler.EndSend(ar);
            Console.WriteLine(
                "Sent {0} bytes to Client", bytesSend);

        }

        internal static void Close(Socket sListener2)
        {
            if (sListener2.Connected)
            {
                sListener2.Shutdown(SocketShutdown.Receive);
                sListener2.Close();
            }

        }

    }
}
