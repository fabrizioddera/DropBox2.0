using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace client.src
{
    class Protocol
    {
        static Socket senderSock;

        internal static string sendProtocol(string theMessageToSend, Socket senderSock2)
        {
            senderSock = senderSock2;
            
            if (theMessageToSend.Contains("SEND"))
            {
                // crea il json della cartella del client
                string value = @"C:\Users\Pc\Desktop\CLIENT";
                var json = CreateJson.GetDirectory(new DirectoryInfo(value));

                request("SEND "+json.ToString());

                return "SEND " + json.ToString();
            }

            else if (theMessageToSend.Contains("SET"))
            {
                string value = @"C:\Users\Pc\Desktop\CLIENT\a.txt";
                int bytesSend = myFiles.sendFile(value, senderSock2);

                return "a.txt";
            }

            else if (theMessageToSend.Contains("GET"))
            {
                request("+GET FILE");

                return "+GET FILE";
            }

            return null;

        }


        internal static string reciveProtocol(String theMessageToReceive, Socket senderSock2)
        {
            bool errore = false;
            string tbReceivedMsg = "";
            senderSock = senderSock2;

            if (theMessageToReceive.Contains("-ERR"))
            {
                tbReceivedMsg += theMessageToReceive;
            }
            else if (theMessageToReceive.Contains("QUIT"))
            {
                tbReceivedMsg += theMessageToReceive;
            }
            else if (theMessageToReceive.Contains("+OK"))
            {
                tbReceivedMsg += theMessageToReceive;
            }
            else if (theMessageToReceive.Contains("+SET UPD"))
            {

                tbReceivedMsg += theMessageToReceive;

                // FILE valido
                if (true)
                {
                    request("+OK\r\n");
                }
                else
                {
                    errore = true;
                }


                // svoglere operazioni di UPD


            }
            else if (theMessageToReceive.Contains("+UPG"))
            {
                tbReceivedMsg += theMessageToReceive;
                request("END");
            }

            else if (theMessageToReceive.Contains("+PUT file"))
            {
                tbReceivedMsg += theMessageToReceive;

                // FILE valido
                if (true)
                {
                    request("+OK\r\n");
                }
                else
                {
                    errore = true;
                }

            }
            else if (theMessageToReceive.Contains("+NO"))
            {
                tbReceivedMsg += theMessageToReceive;
                request("END");
            }


            if (errore)
            {
                request("-ERR\r\n");
            }


            return tbReceivedMsg;

        }


        internal static void request(string str)
        {
            // invia il json al server
            byte[] msg = Encoding.UTF8.GetBytes(str);

            // Sends data to a connected Socket. 
            int bytesSend = senderSock.Send(msg);
        }


    }
}
