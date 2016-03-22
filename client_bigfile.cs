public partial class Form1 : Form
{
    string m_splitter = "'\\'";
    string m_fName;
    string[] m_split = null;
    byte[] m_clientData;

    public Form1()
    {
        InitializeComponent();
    }

    private void sendButton_Click(object sender, EventArgs e)
    {
        char[] delimiter = m_splitter.ToCharArray();

        // Show the open file dialog to select our data.
        openFileDialog1.ShowDialog();

        //Get the file name and write it into our text box.
        textBox1.Text = openFileDialog1.FileName;

        m_split = textBox1.Text.m_split(delimiter);
        int limit = m_split.Length;
  
        m_fName = m_split[limit - 1].ToString();
        
  if (textBox1.Text != null)  button1.Enabled = true;
    }

    private void button1_Click(object sender, EventArgs e)
    {

        Socket clientSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


        byte[] fileName = Encoding.UTF8.GetBytes(m_fName); //file name
        byte[] fileData = File.ReadAllBytes(textBox1.Text); //file
        byte[] fileNameLen = BitConverter.GetBytes(fileName.Length); //lenght of file name
        m_clientData = new byte[4 + fileName.Length + fileData.Length];

        fileNameLen.CopyTo(m_clientData, 0);
        fileName.CopyTo(m_clientData, 4);
        fileData.CopyTo(m_clientData, 4 + fileName.Length);


        clientSock.Connect("192.168.2.10", 9050); //target machine's ip address and the port number
        clientSock.Send(m_clientData);
        clientSock.Close();
    }
}
