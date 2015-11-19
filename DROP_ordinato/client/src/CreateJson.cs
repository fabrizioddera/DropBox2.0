using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client.src
{
    class CreateJson
    {

         //string value = @"C:\Users\Pc\Desktop\Nuova cartella";
         //var json = GetDirectory(new DirectoryInfo(value)).ToString();
         //Console.WriteLine(json);


        JToken GetDirectory(DirectoryInfo directory)
        {
            return JToken.FromObject(new
            {
                directory = directory.EnumerateDirectories().ToDictionary(x => x.Name, x => GetDirectory(x)),
                file = directory.EnumerateFiles().Select(x => x.Name).ToList()
            });
        }

    }
}
