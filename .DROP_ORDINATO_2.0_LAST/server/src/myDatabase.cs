using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace server.src
{
    class myDatabase
    {

        TextBox tbStatus2;
        string db_path = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\Pc\Documents\Visual Studio 2013\Projects\.DROP_ORDINATO_2.0_LAST\server\Database.mdf;Integrated Security=True";

        public void TryCreateTable(TextBox s)
        {
            tbStatus2 = s;
            using (SqlConnection con = new SqlConnection(db_path))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand(
                        "CREATE TABLE dropbox (ID INT, Name VARCHAR(50), Date DATE, Byte INT, MD5 VARCHAR(50), Path VARCHAR(50))", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    tbStatus2.Text = "Table not created.";
                }
            }
        }

        public void AddFile(int ID, string name, DateTime date, int bytes, string md5, string path)
        {
            using (SqlConnection con = new SqlConnection(db_path))
            {
                con.Open();
                if (SearchFile(name) == 0)
                {
                    try
                    {
                        using (SqlCommand command = new SqlCommand(
                            "INSERT INTO dropbox VALUES(@ID, @Name, @Date, @Byte, @MD5, @Path)", con))
                        {
                            command.Parameters.Add(new SqlParameter("ID", ID));
                            command.Parameters.Add(new SqlParameter("Name", name));
                            command.Parameters.Add(new SqlParameter("Date", date));
                            command.Parameters.Add(new SqlParameter("Byte", bytes));
                            command.Parameters.Add(new SqlParameter("MD5", md5));
                            command.Parameters.Add(new SqlParameter("Path", path));
                            command.ExecuteNonQuery();
                        }
                    }
                    catch
                    {
                        tbStatus2.Text = "Count not insert.";
                    }
                }
                else
                {
                    tbStatus2.Text = "Count not insert. Element already exist";
                }
            }
        }

        public void RemoveFile(int ID, string name, DateTime date, int bytes, string md5, string path)
        {
            using (SqlConnection con = new SqlConnection(db_path))
            {
                con.Open();
                try
                {
                    using (SqlCommand command2 = new SqlCommand(
                        "DELETE FROM dropbox  WHERE Name=@Name", con))
                    {
                        command2.Parameters.Add(new SqlParameter("Name", name));
                        command2.ExecuteNonQuery();
                    }
                }
                catch
                {
                    tbStatus2.Text = "Count not delete.";
                }
            }
        }

        public void DisplayFile()
        {
            List<myFiles> files = new List<myFiles>();
            using (SqlConnection con = new SqlConnection(db_path))
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
                        string path = reader.GetString(5);
                        files.Add(new myFiles() { ID = id, Name = name, Date = date, Byte = bytes, MD5 = md5, Path = path });
                    }
                }
            }
            foreach (myFiles file in files)
            {
                tbStatus2.Text = file.ID + "\n" + file.Name + "\n" + file.Date + "\n" + file.Byte + "\n" + file.MD5 + "\n" + file.Path;
            }
        }

        public int SearchFile(String Name)
        {
            List<myFiles> files = new List<myFiles>();
            using (SqlConnection con = new SqlConnection(db_path))
            {
                con.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM dropbox WHERE Name=@Name", con))
                {
                    command.Parameters.Add(new SqlParameter("Name", Name));
                    command.ExecuteNonQuery();

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        DateTime date = reader.GetDateTime(2);
                        int bytes = reader.GetInt32(3);
                        string md5 = reader.GetString(4);
                        string path = reader.GetString(5);
                        files.Add(new myFiles() { ID = id, Name = name, Date = date, Byte = bytes, MD5 = md5, Path = path });
                    }
                    if (files.Count > 0)
                    {
                        return 1;
                    }
                }
            }
            foreach (myFiles file in files)
            {
                tbStatus2.Text = file.ID + "\n" + file.Name + "\n" + file.Date + "\n" + file.Byte + "\n" + file.MD5 + "\n" + file.Path;
            }
            return 0;
        }
    }
}
