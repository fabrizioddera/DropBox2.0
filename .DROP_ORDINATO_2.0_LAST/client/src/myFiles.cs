using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace client.src
{
    class myFiles
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public long Byte { get; set; }
        public string MD5 { get; set; }
        public string Path { get; set; }

        public myFiles(FileInfo file)
        {
            this.ID = 1;
            this.Name = file.Name;
            this.Date = file.LastAccessTime;
            this.Byte = file.Length;
            this.Path = file.FullName;

            FileStream stream = File.Open(file.FullName, FileMode.Open);
            StreamReader reader = new StreamReader(stream);
            this.MD5 = CreateMD5.GetMD5HashData(reader.ReadToEnd().ToString());
            reader.Close();
        }

        public override string ToString()
        {
            return string.Format("ID: {0}, Name: {1}, Date: {2}, Byte: {3}, MD5: {4}, Path: {5}",
                ID, Name, Date, Byte, MD5, Path);
        }

        internal static JToken GetJsonFile(myFiles file)
        {
            return JToken.FromObject(new
            {
                ID = file.ID,
                Name = file.Name,
                Date = file.Date,
                Byte = file.Byte,
                MD5 = file.MD5,
                Path = file.Path
            });
        }

        internal static int sendFile(string value, Socket senderSock2)
        {
            string m_fName = "a.txt";
            byte[] m_clientData;

            byte[] fileName = Encoding.UTF8.GetBytes(m_fName); //file name
            byte[] fileData = File.ReadAllBytes(value); //file
            byte[] fileNameLen = BitConverter.GetBytes(fileName.Length); //lenght of file name
            m_clientData = new byte[4 + fileName.Length + fileData.Length];

            fileNameLen.CopyTo(m_clientData, 0);
            fileName.CopyTo(m_clientData, 4);
            fileData.CopyTo(m_clientData, 4 + fileName.Length);



            // Sends data to a connected Socket. 
            return senderSock2.Send(m_clientData);
        }

        
    }
}
