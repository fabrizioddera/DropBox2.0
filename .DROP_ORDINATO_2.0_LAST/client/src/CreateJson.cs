using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client.src
{
    public class CreateJson
    {
        //string path = @"C:\Users\Pc\Desktop\Nuova cartella";
        //if (System.IO.Directory.Exists(path) == false)
        //{
        //    DirectoryInfo di = Directory.CreateDirectory(path);
        //}
        //var json = CreateJson.GetDirectory(new DirectoryInfo(path)).ToString();
        //Console.WriteLine(json);

        internal static JToken GetDirectory(DirectoryInfo directory)
        {
            return JToken.FromObject(new
            {
                Directories = directory.EnumerateDirectories().ToDictionary(x => x.Name, x => GetDirectory(x)),
                Files = directory.EnumerateFiles().Select(x => GetFile(x)).ToList()
            });
        }

        internal static JToken GetFile(FileInfo file)
        {
            return myFiles.GetJsonFile(new myFiles(file));
        }
    }
}
