using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;
using MetroFramework.Forms;
using Choi_01.weatherApi;
using System.Xml;
using System.Diagnostics;
using MetroFramework;
using System.Runtime.InteropServices;

namespace Choi_01
{

    public partial class FormChat : Form
    {
        //폼 둥굴게 만들기.
        [DllImport("GDI32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
             int nLeftRect, // x-coordinate of upper-left corner
             int nTopRect, // y-coordinate of upper-left corner
             int nRightRect, // x-coordinate of lower-right corner
             int nBottomRect, // y-coordinate of lower-right corner
             int nWidthEllipse, // height of ellipse
             int nHeightEllipse // width of ellipse
        );
      
        //public const int WM_NCLBUTTONDOWN = 0xA1;
        //public const int HT_CAPTION = 0x2;

        //[DllImportAttribute("user32.dll")]
        //public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        //[DllImportAttribute("user32.dll")]
        //public static extern bool ReleaseCapture();

        DoubleBufferPanel panel;
        
        //채팅 사용
        public Dictionary<TcpClient, string> clientList = new Dictionary<TcpClient, string>();
        private string name, pass;  //loginform 에서 들어온데이터를 초기화 하기위해서 만든 변수. 
        TcpClient clientSocket = new TcpClient();
        NetworkStream stream = default(NetworkStream);
        Emoticon emoticon;
        Boolean eFlag = false;

        //날씨 api 사용
        private weather weather = null;

        //뉴스 api rss
        Label[] NText;
        XmlDocument doc;
        XmlNodeList title;
        XmlNodeList link;

        
     

        internal void eSend(Image image)
        {
            if (MessageBox.Show("선택한 이모티콘을 보내시겠습니까? ","", MessageBoxButtons.YesNo) ==DialogResult.Yes)
            {
                Bitmap myBitmap = new Bitmap(image, new Size(100, 100));
                Clipboard.SetDataObject(myBitmap);
                DataFormats.Format format = DataFormats.GetFormat(DataFormats.Bitmap);
                if (richText.CanPaste(format))
                {
                    richText.SelectionAlignment = HorizontalAlignment.Right;
                    richText.Paste(format);
                    richText.AppendText("\n");
                    richText.ScrollToCaret();
                }
                if (!(btnConn.Enabled))
                {
                    try
                    {
                        byte[] byteArray = BitmapToByte(myBitmap);

                        //byte[] a = Encoding.Unicode.GetBytes("");
                        //byte[] newArray = new byte[byteArray.Length + a.Length];
                        //byteArray.CopyTo(newArray, 0);
                        //a.CopyTo(newArray, byteArray.Length);
                        //System.Buffer.BlockCopy(byteArray, 0, newArray,0, byteArray.Length);
                        //System.Buffer.BlockCopy(a, 0, newArray, byteArray.Length, a.Length);
                        //stream.Write(newArray, 0, newArray.Length);

                        stream.Write(byteArray, 0, byteArray.Length);
                        //byte[] bStream = ImageToByte(myBitmap);
                        //NetworkStream nStream = clientSocket.GetStream();
                        //nStream.Write(bStream, 0, bStream.Length);
                        //nStream.Flush();
                        stream.Flush();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }

                }
            }
            else
            {
                MessageBox.Show("이미지를 로드하지 못했습니다.");
            }
        }//이모티콘
        public FormChat()
        {
            InitializeComponent();
            
        
        }
        public FormChat(string a, string b)
        {
            InitializeComponent();
            name = a;
            pass = b;
        Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 40, 40));

        }
      
        private void FormChat_Load(object sender, EventArgs e)
        {

            //textId.Font = new Font("Arial",15,FontStyle.Bold);
            //loginform의 id,pw,port 입력받은 값을 가져온다.
            //GraphicsPath gp = new GraphicsPath();
            //gp.AddEllipse(0,0,488,616);
            //gp.AddRectangle
            //this.Region = new Region(gp);
            trackBar1.Value = 7;
            panel = new DoubleBufferPanel();
            panel.Size = new Size(1400, 600);
            panel.BackgroundImage = Image.FromFile("...\\...\\Resources\\realfinal1.jpg");
            panel1.Controls.Add(panel);

            //pnews1 = new DoubleBufferPanel();
            //pnews1.Size = new Size(500, 500);
            //pnews1.BackgroundImage = Image.FromFile("...\\...\\Resources\\news.gif");
            //pnews1.BackColor = ColorTranslator.FromHtml("#e1d5c9");
            //pnews1.Location = new Point(900, -500);
            //pnews1.BackgroundImageLayout = ImageLayout.Stretch;
            ////pnews1.BackColor = Color.Transparent;
            //panel.Controls.Add(pnews1);

            //Pset = new DoubleBufferPanel();
            //Pset.Size = new Size(160, 60);
            //Pset.Location = new Point(1415,112);
            //Pset.BackColor = Color.Transparent;
            
            //Pset.Controls.Add(trackBar1);
            //panel.Controls.Add(Pset);

            //send.BackColor = System.Drawing.Color.FromArgb(255, 236, 66);
            //btnConn.BackColor = System.Drawing.Color.FromArgb(255, 236, 66);
            richText.BackColor = ColorTranslator.FromHtml("#6a8467");
            //noti = new notify();
            //noti.Show();
            emoticon = new Emoticon(this);

            //FormLogin LoginForm = new FormLogin();  //로그인 폼
            
        }

        private void GetMessage() //메세지를 받는 메소드
        {
            while (true)
            {
                try
                {
                    stream = clientSocket.GetStream();     //stream의 형태로 데이터를 받음
                    int BUFFERSIZE = clientSocket.ReceiveBufferSize;    //수신데이터의 버퍼 크기를 가져온다.
                    byte[] buffer = new byte[BUFFERSIZE];               //수신데이터의 크기로 byte를 만든다.
                    
                    int bytes = stream.Read(buffer, 0, buffer.Length);  //buffer에 stream 읽은 것은 저장한다. 그리고 bytes의 크기 저장
                    string message = Encoding.Unicode.GetString(buffer, 0, bytes);  //buffer을 문자로 변환 0부터 바이트 크기까지
                 
                        if (message.Contains("$") == true)
                        {
                            String msg = message.Substring(0, message.IndexOf("$"));
                            DisplayText(msg);
                      
                        }//richtext에 메시지 출력
                        else if (buffer[0] == 0xFF && buffer[1] == 0xD8)  //jpg의 매직넘버(고유 숫자)   FF D8
                        {
                            TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));
                            Bitmap b = (Bitmap)tc.ConvertFrom(buffer);
                            ImageDisplay(b);
                        }
                        richText.SelectionStart = richText.Text.Length;
                    
                }
                catch
                {
                    break;
                }
                
            }
          
        }
        //private void GetImage()
        //{
        //    while(true)
        //    {
        //        try
        //        {
        //            stream = clientSocket.GetStream();
        //            int BUFFERSIZE = clientSocket.ReceiveBufferSize;
        //            byte[] buffer = new byte[BUFFERSIZE];
        //            int bytes = stream.Read(buffer, 0, buffer.Length);
        //            TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));
        //            Bitmap b = (Bitmap)tc.ConvertFrom(buffer);
        //            Clipboard.SetDataObject(b);
        //            DataFormats.Format format = DataFormats.GetFormat(DataFormats.Bitmap);
        //            if (richText.CanPaste(format))
        //            {

        //                richText.Paste(format);
        //                richText.AppendText("\n");
        //            }
        //            else { MessageBox.Show("x"); }

        //        }
        //        catch
        //        {
        //            break;
        //        }
        //    }
        //}
        private void ImageDisplay(Bitmap imageBitmap)
        {
            if (richText.InvokeRequired)
            {
               
                richText.BeginInvoke(new MethodInvoker(delegate
                {
                    richText.SelectionAlignment = HorizontalAlignment.Left;
                    //Image returnImage = Image.FromStream(ms);
                    //Bitmap myBitmap = new Bitmap(returnImage, new Size(50, 50));
                    //Clipboard.SetDataObject(myBitmap);
                    Clipboard.SetDataObject(imageBitmap);
                    DataFormats.Format format = DataFormats.GetFormat(DataFormats.Bitmap);
                    if (richText.CanPaste(format))
                    {
                        richText.AppendText("\n");
                        richText.Paste(format);
                        richText.AppendText("\n\n");
                    }
                    else { MessageBox.Show("x"); }

                    richText.ScrollToCaret();
                    
                }
                ));
            }
        }
          
        private void FormChat_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
            //FormLogin LoginForm = new FormLogin();
            //LoginForm.Visible = true;
            
            
        }   
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length != 0)
            {
                send.Enabled = true;
                send.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            }
            else
            {
                
                send.Enabled = false;
                send.ForeColor = System.Drawing.Color.FromArgb(250, 250, 250);
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

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            
            //this.Opacity = trackBar1.Value /5;

            this.Opacity =(double)trackBar1.Value / 7.0;
                

        } //투명도

        private void richText_Enter(object sender, EventArgs e)
        {
            textBox1.Focus(); //richtext 커서 뺏기 
        }

        private void FormChat_Move(object sender, EventArgs e)
        {
            if(emoticon!=null)
            emoticon.Visible = false;
        }

        private void btnConn_Click_1(object sender, EventArgs e)
        {
            //try
            //{
            //    clientSocket.Connect("192.168.219.124", 9999);  //해당ip와 포트에 client 접속
            //    //clientSocket.Connect("172.30.1.39", 9999);  //해당ip와 포트에 client 접속
            //    stream = clientSocket.GetStream();              //클라이언트의 스트림을 읽음
            //    richText.SelectionAlignment = HorizontalAlignment.Left;
            //    byte[] buffer = Encoding.Unicode.GetBytes(this.name + "$");   //id+$ 의 문자를 바이트의 형태로 바꿈
            //    stream.Write(buffer, 0, buffer.Length);                     //바이트형태로 바꾼 buffer를 stream에 데이터를 씀
            //    stream.Flush();                                             //즉 id +$ 를 보내는 거임

            //    Thread t_handler = new Thread(GetMessage);          //getMessage메소드를 쓰레드 실행  상대가 보낼때마다 알아서 실행되야하므로 쓰레드 실행
            //    t_handler.IsBackground = true;                 //백그라운드로 설정함으로써 메인메소드가 종료되면 같이 종료된다.
            //    t_handler.Start();
            //    //쓰레드 실행
            //}
            //catch
            //{
            //    MessageBox.Show("서버가 생성되지 않은 상태입니다.", "Error!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}
            //btnConn.Enabled = false;
            //btnConn.Visible = false;

        }

        private void DisplayText(string text)
        {

            if (this.WindowState == FormWindowState.Minimized)
            {


            }
            if (richText.InvokeRequired)
            {
              
                richText.BeginInvoke(new MethodInvoker(delegate
                {
                    richText.SelectionAlignment = HorizontalAlignment.Left;
                    richText.AppendText(text + Environment.NewLine);
            
                }));
            }
            else
                richText.AppendText(text + Environment.NewLine);

            
            richText.ScrollToCaret();
            
        }
     
        //뉴스 rss사용
        //private void metroTabPage4_MouseClick(object sender, MouseEventArgs e)
        //{
        //    MessageBox.Show("뉴스창으로 이동합니다. ");
        //    string[] num = Convert.ToString(sender).Split(':');
        //    string[] num1 = num[num.Length - 1].Split('.');
        //    int count = Convert.ToInt32(num1[0]);
        //    Process.Start(link[count + 1].InnerText);
        //}

        //private void metroTabPage3_Enter(object sender, EventArgs e) //날씨
        //{
        //    metroTabPage3.BackColor = ColorTranslator.FromHtml("#4fb79c");
        //}

        //private void metroTabPage5_Enter(object sender, EventArgs e) //캘린더
        //{
        //    metroTabPage5.BackColor = ColorTranslator.FromHtml("#6c4726");
        //}

        //private void metroTabPage6_Enter(object sender, EventArgs e) //설정
        //{
        //    metroTabPage6.BackColor = ColorTranslator.FromHtml("#8c4b4f");
        //}

        private void tm_Tick(object sender, EventArgs e)
        {
            if (panel1.Location.X < -720) tm.Stop();
            else panel1.Location = new Point(panel1.Location.X - 91, panel1.Location.Y);
        }

        //private void btnChat_Enter(object sender, EventArgs e)
        //{
        //    btnChat.Enter += new EventHandler(metroTabPage2_Enter);
        //}

        //private void btnNews_Enter(object sender, EventArgs e)
        //{
        //    btnNews.Enter += new EventHandler(metroTabPage4_Enter);
        //}

        //private void btnControl_Enter(object sender, EventArgs e)
        //{
        //    btnControl.Enter += new EventHandler(metroTabPage6_Enter);
        //}

        private void tm1_Tick(object sender, EventArgs e)
        {
            if (panel1.Location.X >-90) tm1.Stop();
            else panel1.Location = new Point(panel1.Location.X + 91, panel1.Location.Y);
        }

        private void pnews1_MouseClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show("뉴스창으로 이동합니다. ");
            string[] num = Convert.ToString(sender).Split(':');
            string[] num1 = num[num.Length - 1].Split('.');
            int count = Convert.ToInt32(num1[0]);
            Process.Start(link[count + 1].InnerText);
        }
      
        private void exit2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void mini2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                if (textId.Text == "")
                    textId.Focus();
                else if (textPw.Text == "")
                    textPw.Focus();
                else
                {
                    tm.Start();
                    name = textId.Text;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
  }
        private void news_Click(object sender, EventArgs e)
        {
            doc = new XmlDocument();
            doc.Load("http://rss.donga.com/total.xml ");

            title = doc.GetElementsByTagName("title");
            link = doc.GetElementsByTagName("link");

            NText = new Label[title.Count - 2];

            for (int i = 0; i < NText.Length; i++)
            {
                NText[i] = new Label();
                NText[i].Location = new Point(0, i * 30);
                NText[i].Size = new Size(400, 20);
                NText[i].Name = "NText" + i.ToString();
                NText[i].TabIndex = i;
                NText[i].Text = (i + 1).ToString() + ". " + title[i + 2].InnerText;
                pnews1.Controls.Add(NText[i]);

                NText[i].MouseClick += new MouseEventHandler(pnews1_MouseClick);
            }

         
        }

        private void send_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\box.JPG");
            Bitmap myBitmap = new Bitmap(image, new Size(100, 100));
            Graphics graphics = Graphics.FromImage(myBitmap);
            Font arialFont = new Font("Arial", 10);
            PointF firstLocation = new PointF(10f, 10f);
            PointF secondLocation = new PointF(10f, 50f);


            if (!(btnConn.Enabled))
            {
                richText.SelectionAlignment = HorizontalAlignment.Right;

                char[] text = new char[textBox1.TextLength + textBox1.TextLength / 10];
                String message = null;

                for (int i = 0; i < textBox1.TextLength; i++)
                {
                    text[i] = textBox1.Text[i];
                    if ((i + 1) % 8 == 0)
                        text[i] = (char)10;
                }
                for (int i = 0; i < text.Length; i++)
                {
                    message += text[i];

                }

                richText.AppendText(message + "\n\n");
                byte[] buffer = Encoding.Unicode.GetBytes(message + "$");//textbox에 입력한 글을 +$와 함께 바이트로 변환 


                //richText.AppendText(textBox1.Text + "\n");

                richText.ScrollToCaret();
                stream.Write(buffer, 0, buffer.Length);             //stream 에 바이트 형태로 된 데이터를 보냄(씀)
                stream.Flush();
            }
            else
            {
                richText.SelectionAlignment = HorizontalAlignment.Right;
                for (int i = 0; i < textBox1.TextLength; i++)
                {

                    richText.Text += textBox1.Text[i];

                    if ((i + 1) % 8 == 0)
                        richText.AppendText("\n");
                }
                richText.AppendText("\n\n");

                //richText.AppendText(textBox1.Text + "\n");
                //richText.Text += this.textBox1.Text+"\n";



            }
            textBox1.Text = "";
            //richText.SelectionStart = richText.Text.Length;

            richText.ScrollToCaret();

            textBox1.Focus();
        }

        private void photo_Click(object sender, EventArgs e)
        {
            FileDialog fDialog = new OpenFileDialog();
            fDialog.Filter = "*.BMP; *.JPG; *.GIF; *.Png)| *.BMP; *.JPG; *.GIF; *.Png";
            DialogResult choice = MessageBox.Show("이미지 전송하는 버튼입니다.", "이미지 전송", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (DialogResult.OK == choice)
            {
                if (fDialog.ShowDialog() == DialogResult.OK)
                {
                    string lstrFile = fDialog.FileName;
                    // DisplayText(lstrFile);  이미지 경로가 잘나오는지 확인하는 주석.
                    Image newImage = Image.FromFile(lstrFile);
                    Bitmap myBitmap = new Bitmap(newImage, new Size(100, 100));
                    Clipboard.SetDataObject(myBitmap);
                    DataFormats.Format format = DataFormats.GetFormat(DataFormats.Bitmap);
                    if (richText.CanPaste(format))
                    {
                        richText.SelectionAlignment = HorizontalAlignment.Right;
                        richText.Paste(format);
                        richText.AppendText("\n");
                        richText.ScrollToCaret();

                        if (!(btnConn.Enabled))
                        {
                            try
                            {
                                byte[] byteArray = BitmapToByte(myBitmap);

                                //byte[] a = Encoding.Unicode.GetBytes("");
                                //byte[] newArray = new byte[byteArray.Length + a.Length];
                                //byteArray.CopyTo(newArray, 0);
                                //a.CopyTo(newArray, byteArray.Length);
                                //System.Buffer.BlockCopy(byteArray, 0, newArray,0, byteArray.Length);
                                //System.Buffer.BlockCopy(a, 0, newArray, byteArray.Length, a.Length);
                                //stream.Write(newArray, 0, newArray.Length);

                                stream.Write(byteArray, 0, byteArray.Length);
                                //byte[] bStream = ImageToByte(myBitmap);
                                //NetworkStream nStream = clientSocket.GetStream();
                                //nStream.Write(bStream, 0, bStream.Length);
                                //nStream.Flush();
                                stream.Flush();

                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
                            }

                        }
                    }
                    else
                    {
                        MessageBox.Show("이미지를 로드하지 못했습니다.");
                    }
                }
                else
                {
                    return;
                }
            }
        }

        private void emo_Click(object sender, EventArgs e)
        {
            eFlag = !eFlag;
            if (eFlag)
                et.Start();
            else
                et1.Start();
            //if (eFlag)
            //{
            //    emoticon.Show();
            //    if (this.Location.X < 1000)
            //        emoticon.Location = new Point(this.Location.X + 498, this.Location.Y + 235);
            //    else
            //        emoticon.Location = new Point(this.Location.X - 335, this.Location.Y + 235);
            //}
            //else
            //    emoticon.Visible = false;

        }

        private void back_Click(object sender, EventArgs e)
        {
            clientSocket.Close();
            tm1.Start();
        }

        private void textBox1_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                send_Click(sender, e);
                textBox1.Clear();
            }
        }

        private void et_Tick(object sender, EventArgs e)
        {
            if (panel1.Location.X < -1060) et.Stop();
            else panel1.Location = new Point(panel1.Location.X - 120, panel1.Location.Y);
        }

        private void et1_Tick(object sender, EventArgs e)
        {
            if (panel1.Location.X > -800) et1.Stop();
            else panel1.Location = new Point(panel1.Location.X + 120, panel1.Location.Y);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\e1.jpg");
            this.eSend(image);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\e2.jpg");
            this.eSend(image);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\e3.jpg");
            this.eSend(image);
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\e4.jpg");
            this.eSend(image);
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\e5.jpg");
            this.eSend(image);
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\e6.jpg");
            this.eSend(image);
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\e7.jpg");
            this.eSend(image);
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\e8.jpg");
            this.eSend(image);
        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\e9.jpg");
            this.eSend(image);
        }

        private void pictureBox11_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\e10.jpg");
            this.eSend(image);
        }

        private void pictureBox12_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\e11.jpg");
            this.eSend(image);
        }

        private void pictureBox13_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\e12.jpg");
            this.eSend(image);
        }

        private void pictureBox14_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\e13.jpg");
            this.eSend(image);
        }

        private void pictureBox15_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\e14.jpg");
            this.eSend(image);

        }

        private void pictureBox16_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\e15.jpg");
            this.eSend(image);
        }

        //private void panel1_MouseDown(object sender, MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Left)
        //    {
        //        ReleaseCapture();
        //        SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        //    }
        //}
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x84:
                    base.WndProc(ref m);
                    if ((int)m.Result == 0x1)
                        m.Result = (IntPtr)0x2;
                    return;
            }
            base.WndProc(ref m);
        }
        private void btnCon_Click(object sender, EventArgs e)
        {
            btnCon.Visible = false;
            try
            {
                FormChat chat = new FormChat();
                clientSocket.Connect("220.66.85.36", 9999);  //해당ip와 포트에 client 접속
                //clientSocket.Connect("192.168.219.158", 9999);                                              //clientSocket.Connect("172.30.1.39", 9999);  //해당ip와 포트에 client 접속
                stream = clientSocket.GetStream();              //클라이언트의 스트림을 읽음
                richText.SelectionAlignment = HorizontalAlignment.Left;
                byte[] buffer = Encoding.Unicode.GetBytes(this.name + "$");   //id+$ 의 문자를 바이트의 형태로 바꿈
                stream.Write(buffer, 0, buffer.Length);                     //바이트형태로 바꾼 buffer를 stream에 데이터를 씀
                stream.Flush();                                             //즉 id +$ 를 보내는 거임

                Thread t_handler = new Thread(GetMessage);          //getMessage메소드를 쓰레드 실행  상대가 보낼때마다 알아서 실행되야하므로 쓰레드 실행
                t_handler.IsBackground = true;                 //백그라운드로 설정함으로써 메인메소드가 종료되면 같이 종료된다.
                t_handler.Start();
                //쓰레드 실행
                tm.Start();
            }
            catch
            {
                MessageBox.Show("서버가 생성되지 않은 상태입니다.", "Error!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            btnConn.Enabled = false;
            btnConn.Visible = false;
       

       
        }

      
        private void combo_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            weather = new weather();
            String[] region = combo.Text.Split(',');
            WeatherData weatherData = weather.Reading(region[1]);

            if (weatherData != null)
            {
                Pweather.Image = weatherData.icon;
                label1.Text = weatherData.ToString();
              
            }
            else
            {
                Pweather.Image = null;
               label1.Text = "확인 할 수 없는 도시입니다.";
            }
            Pweather.Refresh();
        }





        //날씨 api
        //private void Combo_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    weather = new weather();
        //    String[] region = combo.Text.Split(',');
        //    WeatherData weatherData = weather.Reading(region[1]);

        //    if (weatherData != null)
        //    {
        //        weatherTile.TileImage = weatherData.icon;
        //        weatherTile.Text = weatherData.ToString();
        //    }
        //    else
        //    {
        //        weatherTile.TileImage = null;
        //        weatherTile.Text = "확인 할 수 없는 도시입니다.";
        //    }
        //    weatherTile.Refresh();
        //}

    }
    
    

}
