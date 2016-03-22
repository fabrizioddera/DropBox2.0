using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    class Protocol
    {

        internal static Socket handler;

        internal static void sendProtocol()
        {

        }

        internal static string[] reciveProtocol(IAsyncResult ar, Socket handler2, byte[] buffer, int flag, int bytesRead)
        {

            handler = handler2;

            // Received message 
            string content = string.Empty;

            // The number of bytes received. 
            //int bytesRead = handler.EndReceive(ar);

            bool errore = false;

		    if (bytesRead > 0)
		    {
			    content += Encoding.UTF8.GetString(buffer, 0, bytesRead);

			    if (content.Contains("QUIT"))
			    {
				    response("QUIT\r\n");
				    // Convert byte array to string
				    var str = content;

				    ////this is used because the UI couldn't be accessed from an external Thread
				    //this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
				    //{
				    //    tbAux.Text = "Read " + str.Length * 2 + " bytes from client.\n Data: " + str;
				    //}
				    //);
			    }
			    else if (content.Contains("-ERR"))
			    {
				    errore = true;
			    }
			    else
			    {
				    if (content.Contains("SEND"))
				    {
					    var str = content.Substring(5);

					    try
					    {
						    JObject json1 = JObject.Parse(str);

						    string path = @"C:\Users\Pc\Desktop\SERVER";
						    if (System.IO.Directory.Exists(path) == false)
						    {
							    // Try to create the directory.
							    DirectoryInfo di = Directory.CreateDirectory(path);


							    // FIXME aggiungere parte database primo avvio



						    }

						    var json2 = CreateJson.GetDirectory(new DirectoryInfo(path));

						    // confonta i due json e ritorna bool
						    if (!JToken.DeepEquals(json1, json2))
						    {
							    response("+OK\r\n");
						    }
						    else
						    {
							    response("+SET UPD\r\n");
						    }
					    }
					    catch (JsonReaderException jex)
					    {
						    errore = true;
					    }
				    }
                    else if (content.Contains("+SET UPD"))
                    {
                        return res(errore, content, false);
                    }
                    else if (content.Contains("+GET FILE"))
                    {
                        var file = content.Substring(5);

                        // FILE valido
                        if (true)
                        {



                            // FIXME ricerca nel database





                            // FILE presente nel db
                            if (true)
                            {
                                response("+PUT file\r\n");
                            }
                            else
                            {
                                response("+NO\r\n");
                            }
                        }
                        else
                        {
                            errore = true;
                        }
                    }
                    else if (content.Contains("OK"))
                    {
                        return res(errore, content, false);
                    }
                    else if (content.Contains(".txt"))
                    {
                        return res(errore, content, true);
                    }
			    }

                return res(errore, content, false);
			    
		    }

            return res(errore, content, false);
        }

        internal static string[] res(bool errore, string content, bool file)
        {
            string[] res = {"", "", "false"};


             if (errore == true)
            {
                res[0] = "true";
            }
            else 
            {
                res[0] = "false";
            }
            if(file)
             {
                res[2] = "file";
             }
            else
            {
                res[2] = "false";
            }
            res[1] = content;

            return res;
        }

        internal static void response(string str)
        {
            // Prepare the reply message 
            byte[] byteData = Encoding.Unicode.GetBytes(str);

            // Sends data asynchronously to a connected Socket 
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(socket_server.SendCallback), handler);
        }

    }
}
