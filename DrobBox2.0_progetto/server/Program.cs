using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace server
{
    class Program
    {
        public void Main()
        {
            TryCreateTable();
            while (true)
            {
                string[] input = Console.ReadLine().Split(' ');
                try
                {
                    char c = char.ToLower(input[0][0]);
                    if (c == 's')
                    {
                        DisplayFile();
                        continue;
                    }
                    int id = int.Parse(input[0]);
                    string name = input[1];
                    DateTime date = DateTime.Parse(input[2]);
                    long bytes = long.Parse(input[3]);
                    string md5 = input[4];
                    AddFile(id, name, date, bytes, md5);
                }
                catch
                {
                    Console.WriteLine("Input error");
                }
            }
        }

        /// <summary>
        /// This method attempts to create the Dogs1 SQL table.
        /// If will do nothing but print an error if the table already exists.
        /// </summary>
        public void TryCreateTable()
        {
            using (SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename='C:\Users\Pc\Documents\Visual Studio 2013\Projects\DrobBox2.0_progetto\server\Database1.mdf';Integrated Security=True"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand(
                        "CREATE TABLE dropbox (ID INT, Name TEXT, Date TEXT, Byte INT, MD5 TEXT)", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Table not created.");
                }
            }
        }

        /// <summary>
        /// Insert dog data into the SQL database table.
        /// </summary>
        /// <param name="weight">The weight of the dog.</param>
        /// <param name="name">The name of the dog.</param>
        /// <param name="breed">The breed of the dog.</param>
        public void AddFile(int ID, string name, DateTime date, long bytes, string md5)
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
                    Console.WriteLine("Count not insert.");
                }
            }
        }

        public void RemoveFile(int ID, string name, DateTime date, long bytes, string md5)
        {
            using (SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename='C:\Users\Pc\Documents\Visual Studio 2013\Projects\DrobBox2.0_progetto\server\Database1.mdf';Integrated Security=True"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand(
                        "DELETE FROM dropbox VALUES(@ID, @Name, @Date, @Byte, @MD5)", con))
                    {
                        command.Parameters.Remove(ID);
                        command.Parameters.Remove(name);
                        command.Parameters.Remove(date);
                        command.Parameters.Remove(bytes);
                        command.Parameters.Remove(md5);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Count not deete.");
                }
            }
        }

        /// <summary>
        /// Read in all rows from the Dogs1 table and store them in a List.
        /// </summary>
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
                        long bytes = reader.GetInt32(3);
                        string md5 = reader.GetString(4);
                        files.Add(new Files() { ID = id, Name = name, Date = date, Byte = bytes, MD5 = md5 });
                    }
                }
            }
            foreach (Files file in files)
            {
                Console.WriteLine(file);
            }
        }
    }

    /// <summary>
    /// Encapsulates data for dog objects.
    /// </summary>
    public class Files
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public long Byte { get; set; }
        public string MD5 { get; set; }

        public override string ToString()
        {
            return string.Format("ID: {0}, Name: {1}, Date: {2}, Byte: {3}, MD5: {4}",
                ID, Name, Date, Byte, MD5);
        }
    }
}
