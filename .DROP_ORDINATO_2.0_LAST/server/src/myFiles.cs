using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace server.src
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

        public myFiles()
        {
            // TODO: Complete member initialization
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

        internal static int reciveFile(IAsyncResult ar, int flag, Socket handler, byte[] buffer, int bytesRead)
        {
            string receivedPath = "";
            int fileNameLen = 1;
            

            // Received message 
            string content = string.Empty;

            // The number of bytes received. 
            //int bytesRead = handler.EndReceive(ar);


            if (bytesRead > 0)
            {

                if (flag == 0)
                {
                    fileNameLen = BitConverter.ToInt32(buffer, 0);
                    string fileName = Encoding.UTF8.GetString(buffer, 4, fileNameLen);
                    receivedPath = @"C:\Users\Pc\Desktop\SERVER\" + fileName;
                    flag++;
                }
                if (flag >= 1)
                {
                    BinaryWriter writer = new BinaryWriter(File.Open(receivedPath, FileMode.Append));
                    if (flag == 1)
                    {
                        writer.Write(buffer, 4 + fileNameLen, bytesRead - (4 + fileNameLen));
                        flag++;
                    }
                    else 
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }
                    writer.Close();
                    return flag;
                }

            }

            return 0;
        }

    }
}
