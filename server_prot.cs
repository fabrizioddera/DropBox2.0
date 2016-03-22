public void response(string str)
{
	// Prepare the reply message 
	byte[] byteData = Encoding.Unicode.GetBytes(str);

	// Sends data asynchronously to a connected Socket 
	handler.BeginSend(byteData, 0, byteData.Length, 0,
		new AsyncCallback(SendCallback), handler);
}


public void ReceiveCallback(IAsyncResult ar)
{
	try
	{
		// Fetch a user-defined object that contains information 
		object[] obj = new object[2];
		obj = (object[])ar.AsyncState;

		// Received byte array 
		byte[] buffer = (byte[])obj[0];

		// A Socket to handle remote host communication. 
		handler = (Socket)obj[1];

		// Received message 
		string content = string.Empty;

		// The number of bytes received. 
		int bytesRead = handler.EndReceive(ar);

		bool errore = false;

		if (bytesRead > 0)
		{
			content += Encoding.Unicode.GetString(buffer, 0, bytesRead);

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

						string path = @"C:\Users\Pc\Desktop\Nuova cartella";
						if (System.IO.Directory.Exists(path) == false)
						{
							// Try to create the directory.
							DirectoryInfo di = Directory.CreateDirectory(path);


							// FIXME aggiungere parte database primo avvio



						}

						var json2 = GetDirectory(new DirectoryInfo(path));

						// confonta i due json e ritorna bool
						if (JToken.DeepEquals(json1, json2))
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
					var upd = content.Substring(5);

					// UPD valido
					if (true)
					{


						// FIXME aggiornamenti database



						response("+UPG\r\n");
					}
					else
					{
						errore = true;
					}
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
				else if (content.Contains("++OK"))
				{

				}
			}


			if (errore)
			{
				response("-ERR\r\n");
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

			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
			{
				tbAux.Text = content;
			}
			);
		}
	}
	catch (Exception exc) { MessageBox.Show(exc.ToString()); }
}
