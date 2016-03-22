using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace server.src
{
    public class Callbacks
    {
        Socket handler;
        int flag = 0;
        
        public void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = null;

            // A new Socket to handle remote host communication 
            Socket handler = null;
            // Receiving byte array 
            byte[] buffer = new byte[1024];
            // Get Listening Socket object 
            listener = (Socket)ar.AsyncState;
            // Create a new socket 
            handler = listener.EndAccept(ar);

            // Using the Nagle algorithm 
            handler.NoDelay = false;

            // Creates one object array for passing data 
            object[] obj = new object[2];
            obj[0] = buffer;
            obj[1] = handler;

            // Begins to asynchronously receive data 
            handler.BeginReceive(
                buffer,        // An array of type Byt for received data 
                0,             // The zero-based position in the buffer  
                buffer.Length, // The number of bytes to receive 
                SocketFlags.None,// Specifies send and receive behaviors 
                new AsyncCallback(ReceiveCallback),//An AsyncCallback delegate 
                obj            // Specifies infomation for receive operation 
                );

            // Begins an asynchronous operation to accept an attempt 
            AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
            flag = 0;
            listener.BeginAccept(aCallback, listener);
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            // Fetch a user-defined object that contains information 
            object[] obj = new object[2];
            obj = (object[])ar.AsyncState;

            // Received byte array 
            byte[] buffer = (byte[])obj[0];

            // A Socket to handle remote host communication. 
            handler = (Socket)obj[1];

            // The number of bytes received. 
            int bytesRead = handler.EndReceive(ar);



            string[] res = Protocol.reciveProtocol(ar, handler, buffer, flag, bytesRead);


            if(res[2] == "file")
            {                
                string content = res[1];
                //var upd = content.Substring(5);
                
                int flag2 = myFiles.reciveFile(ar, flag, handler, buffer, bytesRead);
                if (flag2 >= 1)
                {
                    handler.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), obj);
                }






				// UPD valido
				if (true)
				{


					// FIXME aggiornamenti database



					Protocol.response("+UPG\r\n");
                }
				else
				{
					//errore = true;
				}
			}
        


            if (res[0] == "true")
            {
                //response("-ERR\r\n");

                // Prepare the reply message 
                byte[] byteData = Encoding.Unicode.GetBytes("-ERR\r\n");

                // Sends data asynchronously to a connected Socket 
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(socket_server.SendCallback), handler);
            }
            else
            {
                //Continues to asynchronously receive data
                byte[] buffernew = new byte[1024];
                obj[0] = buffernew;
                obj[1] = handler;
                handler.BeginReceive(buffernew, 0, buffernew.Length,
                        SocketFlags.None, new AsyncCallback(ReceiveCallback), obj);
            }

            //this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            //{
            //    tbAux.Text = res[1];
            //}
            //);

        }
    }
}
