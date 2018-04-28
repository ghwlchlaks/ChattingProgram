using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Drawing.Imaging;

namespace server1
{
    public partial class Form1 : Form
    {
        TcpListener server = null;
        TcpClient clientSocket = null;
        static int counter = 0;
        public Dictionary<TcpClient, string> clientList = new Dictionary<TcpClient, string>();

        public Form1()
        {
            InitializeComponent();
            
            // 메인 메소드와 상관없이 수신대기를 해야하므로 쓰레드를 이용한다. 
            Thread t = new Thread(InitSocket);  //initSocket메소드를 이용해 쓰레드를 실행한다. 
            t.IsBackground = true;              //백그라운드 쓰레드: 메인 쓰레드 종료시 같이 종료된다. 
            t.Start();                          //쓰레드 실행.
           

        }
        private void InitSocket()
        {
            server = new TcpListener(IPAddress.Any, 9999);       //모든 네트워크,포트 9999상태로 수신대기함.
            clientSocket = default(TcpClient);                  //tcpclient에대한 연결음함./.
            server.Start();                                        //수신시작.
            Display("\n          서버가 시작되었습니다. \n");

            while (true)
            {
                try
                {
                    counter++;  //접속한 클라이언트의 수 증가
                    clientSocket = server.AcceptTcpClient(); //데이터를 교환할수 있는  클라이언트 소켓 
                    

                    NetworkStream stream = clientSocket.GetStream();    //stream을 읽음
                    byte[] buffer = new byte[1024];                     //byte 생성
                    int bytes = stream.Read(buffer, 0, buffer.Length);  //byte에다가 stream을 넣고 그 크기를 byte에 저장
                    string user_name = Encoding.Unicode.GetString(buffer, 0, bytes);    //user_name이라는 string에 buffer을 문자로 바꿔서 저장
                    user_name = user_name.Substring(0, user_name.IndexOf("$"));//아이디가 클라이언트에서 데이터를 보낼때 $기호와
                    //함께오는데 0번째 인덱스부터 $가나오는 기호까지의 문자열을 user_name의 저장 (왜하는지 모르겠음...)
                    Display("접속한 클라이언트 : " + user_name);

                    clientList.Add(clientSocket, user_name); //dictionary 로 만든 컬렉션에다가 clientsocket username을 저장

                    SendMessageAll(user_name + "님이 입장하셨습니다.\n", "", false); //모든 클라이언트에게 메시지 전달.
                    //(어떤아이디를 가진 클라이언트가 들어왔을때 모든사람에게 전달하는 메소드. )

                    handleClient h_client = new handleClient();   //handleClient라는 클래스의 객체 생성
                    h_client.OnReceived += new handleClient.MessageDisplayHandler(OnReceived);
                    h_client.OnImageReceived += new handleClient.ImageDisplayHandler(OnImageReceived);
                    h_client.OnDisconnected += new handleClient.DisconnectedHandler(h_client_OnDisconnected);
                    h_client.startClient(clientSocket, clientList);
                }
                catch (Exception ex)
                {
                    Display(ex.Message);
                    break;
                }
            }
            //오류 발생시 while문을 빠져나오게되는데 그때 데이터를 주고 받는 클라이언트 소켓을 차단하고 
            //서버 소켓을 중지한다. 
            clientSocket.Close();
            server.Stop();
        }
        void h_client_OnDisconnected(TcpClient clientSocket)
        {
            if (clientList.ContainsKey(clientSocket))
            {
                String d_name = null;
                clientList.TryGetValue(clientSocket, out d_name);
                Display(d_name + " 님이 퇴장하셨습니다.");
                SendMessageAll(d_name + "님이 퇴장하셨습니다.", d_name, false);
                clientList.Remove(clientSocket);
             }
        }
        private void OnReceived(string message, string user_name)
        {
            string displayMessage = "From client : " + user_name + " : " + message + "\n";
            Display(displayMessage);
            SendMessageAll(message, user_name, true);
        }

        //private void OnImageReceived(string user_name, MemoryStream ms)
          private void OnImageReceived(string user_name, Image image)
        {
            //Image newImage = Image.FromStream(Istream); //exception occurs her
            Bitmap mybitmap = new Bitmap(image, new Size(100, 100));
            //Clipboard.SetDataObject(mybitmap);
            //DataFormats.Format format = DataFormats.GetFormat(DataFormats.Bitmap);
            string displayMessage = "From client : " + user_name + " : \n" ;
            Display(displayMessage);
            ImageDisplay(mybitmap, user_name);
            SendImageAll(mybitmap, user_name, true);
            //ImageDisplay(ms, user_name);

        }
        public void SendImageAll(Bitmap bitmap,string user_name,bool flag)
        {
            foreach (var pair in clientList)
            {
                if (pair.Value != user_name)
                {
                    TcpClient client = pair.Key as TcpClient;
                    NetworkStream stream = client.GetStream();
                    byte[] buffer = null;
                    byte[] buffer1 = null;

                    if (flag)
                    {
                        buffer = BitmapToByte(bitmap);
                        buffer1 = Encoding.Unicode.GetBytes(user_name+" says : $");
                    }
                    else
                    {
                        buffer = Encoding.Unicode.GetBytes("\n");
                    }
                    stream.Write(buffer1, 0, buffer1.Length);
                    stream.Flush();
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                }
            }
        }
        private byte[] BitmapToByte(Bitmap mybitmap)
        {

            byte[] imageData;
            using (var stream = new MemoryStream())
            {
                mybitmap.Save(stream, ImageFormat.Jpeg);


                imageData = stream.ToArray();

                return imageData;

                //MemoryStream mMemoryStream = new MemoryStream();
                //mybitmap.Save(mMemoryStream, ImageFormat.Png);
                //return mMemoryStream.ToArray();
            }
        }
        public void SendMessageAll(string message, string user_name, bool flag)
        {
            try
            {
                foreach (var pair in clientList)
                {

                    if (pair.Value != user_name)
                    {
                        TcpClient client = pair.Key as TcpClient;
                        NetworkStream stream = client.GetStream();
                        byte[] buffer = null;

                        if (flag)
                        {
                            buffer = Encoding.Unicode.GetBytes(user_name + " says : \n" + message + "\n$");
                        }
                        else
                        {
                            buffer = Encoding.Unicode.GetBytes(message + "\n$");
                        }

                        stream.Write(buffer, 0, buffer.Length);
                        stream.Flush();
                    }
                }
            }
            catch
            {
                MessageBox.Show("클라이언트가 모두 종료되었습니다 .. 서버를 종료합니다.!");
                server.Stop();
                
            }
        }
        private void Display(string text)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke(new MethodInvoker(delegate
                {
                    richTextBox1.AppendText(text +"\n");
                    richTextBox1.ScrollToCaret();
                }));
            }
            else
                richTextBox1.AppendText(text + "\n");
            
        }

        //private void ImageDisplay(MemoryStream ms, string user_name)
        private void ImageDisplay(Bitmap imageBitmap, string user_name)
        {
            if(richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke(new MethodInvoker(delegate
                {
                    //Image returnImage = Image.FromStream(ms);
                    //Bitmap myBitmap = new Bitmap(returnImage, new Size(50, 50));
                    //Clipboard.SetDataObject(myBitmap);
                    Clipboard.SetDataObject(imageBitmap);
                    DataFormats.Format format = DataFormats.GetFormat(DataFormats.Bitmap);
                    if (richTextBox1.CanPaste(format))
                    {
                        
                        richTextBox1.Paste(format);
                        richTextBox1.AppendText("\n");
                        richTextBox1.ScrollToCaret();
                    }
                    else { MessageBox.Show("x"); }

                    
                }
                ));
            }
        }

        private void richTextBox1_Enter(object sender, EventArgs e)
        {
            textBox1.Focus();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox1.BackColor =ColorTranslator.FromHtml("#6a8467");
        }
    }
    class handleClient
        {
            TcpClient clientSocket = null;
            public Dictionary<TcpClient, string> clientList = null;

            public void startClient(TcpClient clientSocket, Dictionary<TcpClient, string> clientList)
            {
                this.clientSocket = clientSocket;
                this.clientList = clientList;

                Thread t_hanlder = new Thread(doChat);
            //t_hanlder.SetApartmentState(ApartmentState.STA);
            t_hanlder.IsBackground = true;
                t_hanlder.Start();
            }

            public delegate void MessageDisplayHandler(string message, string user_name);
            public event MessageDisplayHandler OnReceived;

        //public delegate void ImageDisplayHandler(string user_name, MemoryStream ms);
        public delegate void ImageDisplayHandler(string user_name, Bitmap b);
        public event ImageDisplayHandler OnImageReceived;

            public delegate void DisconnectedHandler(TcpClient clientSocket);
            public event DisconnectedHandler OnDisconnected;

            private void doChat()
            {
                NetworkStream stream = null;
                try
                {
                    byte[] buffer = new byte[clientSocket.ReceiveBufferSize];
                    string msg = string.Empty;
                    int bytes = 0;
                    int MessageCount = 0;

                while (true)
                {
                    MessageCount++;
                    stream = clientSocket.GetStream();
                    bytes = stream.Read(buffer, 0, buffer.Length);
                    msg = Encoding.Unicode.GetString(buffer, 0, bytes);

                    
                    if (msg.Contains("$") == true)   //문자인지 구별 
                    {
                        
                        msg = msg.Substring(0, msg.IndexOf("$"));
                        if (OnReceived != null)
                            OnReceived(msg, clientList[clientSocket].ToString());
                    }
                    else if(buffer[0]==0xFF&&buffer[1]==0xD8)  //jpg의 매직넘버(고유 숫자)   FF D8
                    {
                        TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));
                        Bitmap b = (Bitmap)tc.ConvertFrom(buffer);

                        if (OnImageReceived != null)
                            OnImageReceived(clientList[clientSocket].ToString(), b);

                    }
                    
                    //else
                    //{
                    //    //byte[] a = new byte[8629];
                    //    //System.Buffer.BlockCopy(buffer, 0, a, 0, 8629);
                    //    //String msg1 = msg.Substring(0, msg.Length-1);
                    //    //byte[] buffer1 = Encoding.Unicode.GetBytes(msg1);
                    //    //msg = null;
                    //    //bytes = 0;
                    //TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));
                    //Bitmap b = (Bitmap)tc.ConvertFrom(buffer);


                    //    //ImageConverter convertData = new ImageConverter();
                    //    //Image image = (Image)convertData.ConvertFrom(buffer);
                    //    //MemoryStream ms = new MemoryStream(buffer);
                    //    //ms.Position = 0;


                    //    //Image image= Bitmap.FromStream(ms);
                    //    //Image image = Image.FromStream(ms);
                    //    //image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                    //    if (OnImageReceived != null)
                    //        OnImageReceived(clientList[clientSocket].ToString(), b);

                    //    msg = null;
                    //    bytes = 0;
                    //    //byte[] newCoverDate = new byte[ms.Length];
                    //    //ms.Read(newCoverDate, 0, Convert.ToInt32(ms.Length));
                    //    //ms = new MemoryStream();
                    //    //ms.Write(newCoverDate, 0, newCoverDate.Length);
                    //    //image = Image.FromStream(ms);

                    //    //OnImageReceived(clientList[clientSocket].ToString(), ms);

                    //    //ms.Flush();
                    //    //stream.Flush();
                    //    //stream.Read(buffer, 0, buffer.Length);
                    //    //  Image returnImage = Image.FromStream(ms);   


                    //    //1. FileStream fs = 
                    //    //new FileStream(@"c:\Images\test.bmp", FileMode.Open, FileAccess.Read); 
                    //    //2. byte[] buff = File.ReadAllBytes(@"c:\Images\test.bmp"); 

                    //}
                }
                  
                }
                catch 
                {
                
                    if (clientSocket != null)
                    {
                    if (OnDisconnected != null)
                        OnDisconnected(clientSocket);
                

                        clientSocket.Close();
                        stream.Close();
                    
                    }
                }
            }

        }

    }

