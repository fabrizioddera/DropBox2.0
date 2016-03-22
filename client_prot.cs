public void request(string str)
{
	// invia il json al server
	byte[] msg = Encoding.Unicode.GetBytes(str);

	// Sends data to a connected Socket. 
	int bytesSend = senderSock.Send(msg);
}

private void Send_Click(object sender, RoutedEventArgs e)
{
	try
	{
		// Sending message 
		string theMessageToSend = "";

		if (theMessageToSend.Contains("QUIT"))
		{
			request("QUIT\r\n");
		}
		else
		{
			if (theMessageToSend.Contains("SEND"))
			{
				// crea il json della cartella del client
				string value = @"C:\Users\Pc\Desktop\Nuova cartella";
				var json = GetDirectory(new DirectoryInfo(value));

				request(json.ToString());
			}

			else if (theMessageToSend.Contains("SET"))
			{
				request("+SET UPD");
			}

			else if (theMessageToSend.Contains("GET"))
			{
				request("+GET FILE");
			}

			ReceiveDataFromServer();
		}
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

		bool errore = false;

		if (theMessageToReceive.Contains("-ERR"))
		{
			tbReceivedMsg.Text += theMessageToReceive;
		}
		else if (theMessageToReceive.Contains("QUIT"))
		{
			tbReceivedMsg.Text += theMessageToReceive;
		}
		else if (theMessageToReceive.Contains("+OK"))
		{
			tbReceivedMsg.Text += theMessageToReceive;
		}
		else if (theMessageToReceive.Contains("+SET UPD"))
		{

			tbReceivedMsg.Text += theMessageToReceive; 
			
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
			tbReceivedMsg.Text += theMessageToReceive;
			request("END");
		}

		else if (theMessageToReceive.Contains("+PUT file"))
		{
			tbReceivedMsg.Text += theMessageToReceive;

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
			tbReceivedMsg.Text += theMessageToReceive;
			request("END");
		}


		if (errore)
		{
			request("-ERR\r\n");
		}


		
	}
	catch (Exception exc) { MessageBox.Show(exc.ToString()); }
}
