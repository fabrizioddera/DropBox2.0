public partial class Form1 : Form
{
    Thread t1;
    int flag = 0;
    string receivedPath = "yok";
    public delegate void MyDelegate();

    public Form1()
    {
        t1 = new Thread(new ThreadStart(StartListening));
        t1.Start();
        InitializeComponent();
    }

    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;

        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
    }

    public static ManualResetEvent allDone = new ManualResetEvent(false);

    public void StartListening()
    {
        byte[] bytes = new Byte[1024];
        IPEndPoint ipEnd = new IPEndPoint(IPAddress.Any, 9050);
        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            listener.Bind(ipEnd);
            listener.Listen(100);
            while (true)
            {
                allDone.Reset();
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                allDone.WaitOne();

            }
        }
        catch (Exception ex)
        {

        }

    }
    public void AcceptCallback(IAsyncResult ar)
    {

        allDone.Set();


        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);


        StateObject state = new StateObject();
        state.workSocket = handler;
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
        new AsyncCallback(ReadCallback), state);
        flag = 0;
    }

    public void ReadCallback(IAsyncResult ar)
    {

        int fileNameLen = 1;
        String content = String.Empty;
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;
        int bytesRead = handler.EndReceive(ar);
        if (bytesRead > 0)
        {

            if (flag == 0)
            {
                fileNameLen = BitConverter.ToInt32(state.buffer, 0);
                string fileName = Encoding.UTF8.GetString(state.buffer, 4, fileNameLen);
                receivedPath = @'C:\Users\Name\Desktop\' + fileName;
            flag++;
        }
        if (flag >= 1)
        {
            BinaryWriter writer = new BinaryWriter(File.Open(receivedPath, FileMode.Append));
            if (flag == 1)
            {
                writer.Write(state.buffer, 4 + fileNameLen, bytesRead - (4 + fileNameLen));
                flag++;
            }
            else
            writer.Write(state.buffer, 0, bytesRead);
            writer.Close();
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);
        }

    }
    else
    {
        Invoke(new MyDelegate(LabelWriter));
    }

}
public void LabelWriter()
{
    label1.Text = "Data has been received";
}
private void Form1_FormClosed(object sender, FormClosedEventArgs e)
{
    t1.Abort();
}

}