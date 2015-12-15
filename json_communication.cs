// CLIENT

// crea il json della cartella del client
string value = @"C:\Users\Pc\Desktop\Nuova cartella";
var json = GetDirectory(new DirectoryInfo(value));

// invia il json al server
byte[] byteData = Encoding.Unicode.GetBytes(json.ToString());


//______________________________________________________________


// SERVER

// crea il json della cartella del server
string value = @"C:\Users\Pc\Desktop\Nuova cartella";
var json = GetDirectory(new DirectoryInfo(value));

// quando riceve la stringa dal client e la converte in json
JObject o1 = JObject.Parse(str);

// confonta i due json e ritorna bool
bool a = JToken.DeepEquals(json, o1);