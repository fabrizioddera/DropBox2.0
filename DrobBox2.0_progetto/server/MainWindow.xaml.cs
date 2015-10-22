using System.Windows;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Receiving byte array  
        byte[] bytes = new byte[1024];
        string filename;

        public MainWindow()
        {
            InitializeComponent();            
        }


        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();
            
            if (result == true)
            {
                // Open document
                filename = dlg.FileName;
                txtPath.Text = filename;
            }

            FileInfo fi = new FileInfo(filename);
            List<Files> items = new List<Files>();
            Files f = new Files() { Name = fi.Name, Date = fi.LastAccessTime, Byte = (int)fi.Length };
            items.Add(f);
            list1.ItemsSource = items;


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        { 
        
            FileInfo fi = new FileInfo(filename);
            TryCreateTable();

            string a = "1 " + fi.Name + " " + fi.LastAccessTime + " " + fi.Length + " " + fi.Name;
            string[] input = a.Split(' ');


            int id = int.Parse(input[0]);
            string name = input[1];
            DateTime date = fi.LastAccessTime;
            int bytes = int.Parse(input[4]); ;
            string md5 = GetMD5HashData(input[5]);

            if (trasferisci.IsChecked == true)
            {
                AddFile(id, name, date, bytes, md5);
                DisplayFile();

            }
            else if (sostituisci.IsChecked == true)
            {
                AddFile(id, name, date, bytes, md5);
                DisplayFile();
            }
            else if (elimina.IsChecked == true)
            {
                RemoveFile(id, name, date, bytes, md5);
                DisplayFile();
            }
            else
            {

            }
        }


        /// <summary>
        /// take any string and encrypt it using MD5 then
        /// return the encrypted data 
        /// </summary>
        /// <param name="data">input text you will enterd to encrypt it</param>
        /// <returns>return the encrypted text as hexadecimal string</returns>
        private string GetMD5HashData(string data)
        {
            //create new instance of md5
            MD5 md5 = MD5.Create();

            //convert the input text to array of bytes
            byte[] hashData = md5.ComputeHash(Encoding.Default.GetBytes(data));

            //create new instance of StringBuilder to save hashed data
            StringBuilder returnValue = new StringBuilder();

            //loop for each byte and add it to StringBuilder
            for (int i = 0; i < hashData.Length; i++)
            {
                returnValue.Append(hashData[i].ToString());
            }

            // return hexadecimal string
            return returnValue.ToString();

        }

        public void TryCreateTable()
        {
            using (SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename='C:\Users\Pc\Documents\Visual Studio 2013\Projects\DrobBox2.0_progetto\server\Database1.mdf';Integrated Security=True"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand(
                        "CREATE TABLE dropbox (ID INT, Name VARCHAR(50), Date DATE, Byte INT, MD5 VARCHAR(50))", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    tesot.Text ="Table not created.";
                }
            }
        }

        public void AddFile(int ID, string name, DateTime date, int bytes, string md5)
        {
            using (SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename='C:\Users\Pc\Documents\Visual Studio 2013\Projects\DrobBox2.0_progetto\server\Database1.mdf';Integrated Security=True"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand(
                        "INSERT INTO dropbox VALUES(@ID, @Name, @Date, @Byte, @MD5)", con))
                    {
                        command.Parameters.Add(new SqlParameter("ID", ID));
                        command.Parameters.Add(new SqlParameter("Name", name));
                        command.Parameters.Add(new SqlParameter("Date", date));
                        command.Parameters.Add(new SqlParameter("Byte", bytes));
                        command.Parameters.Add(new SqlParameter("MD5", md5));
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    tesot.Text ="Count not insert.";
                }
            }
        }

        public void RemoveFile(int ID, string name, DateTime date, int bytes, string md5)
        {
            using (SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename='C:\Users\Pc\Documents\Visual Studio 2013\Projects\DrobBox2.0_progetto\server\Database1.mdf';Integrated Security=True"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command2 = new SqlCommand(
                        "DELETE FROM dropbox VALUES(@ID, @Name, @Date, @Byte, @MD5) WHERE Name=@Name", con))
                    {
                        command2.Parameters.Remove(ID);
                        command2.Parameters.Remove(name);
                        command2.Parameters.Remove(date);
                        command2.Parameters.Remove(bytes);
                        command2.Parameters.Remove(md5);
                        command2.ExecuteNonQuery();
                    }
                }
                catch
                {
                    tesot.Text = "Count not delete.";
                }
            }
        }

        public void DisplayFile()
        {
            List<Files> files = new List<Files>();
            using (SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename='C:\Users\Pc\Documents\Visual Studio 2013\Projects\DrobBox2.0_progetto\server\Database1.mdf';Integrated Security=True"))
            {
                con.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM dropbox", con))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        DateTime date = reader.GetDateTime(2);
                        int bytes = reader.GetInt32(3);
                        string md5 = reader.GetString(4);
                        files.Add(new Files() { ID = id, Name = name, Date = date, Byte = bytes, MD5 = md5 });
                    }
                }
            }
            foreach (Files file in files)
            {
                tesot.Text = file.ID + "\n" + file.Name + "\n" + file.Date + "\n" + file.Byte + "\n" + file.MD5;
            }
        }

        public class Files
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public DateTime Date { get; set; }
            public int Byte { get; set; }
            public string MD5 { get; set; }

            public override string ToString()
            {
                return string.Format("ID: {0}, Name: {1}, Date: {2}, Byte: {3}, MD5: {4}",
                    ID, Name, Date, Byte, MD5);
            }
        }


    }
}
