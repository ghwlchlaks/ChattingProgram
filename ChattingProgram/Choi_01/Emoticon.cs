using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Choi_01
{
    public partial class Emoticon : MetroForm
    { 
        private FormChat formChat;

        public Emoticon()
        {
            InitializeComponent();
        }

        public Emoticon(FormChat formChat)
        {
            InitializeComponent();
            this.formChat = formChat;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\1.jpg");
            formChat.eSend(image);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\2.jpg");
            formChat.eSend(image);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\3.jpg");
            formChat.eSend(image);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\4.jpg");
            formChat.eSend(image);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\5.jpg");
            formChat.eSend(image);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\6.jpg");
            formChat.eSend(image);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\7.jpg");
            formChat.eSend(image);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\8.jpg");
            formChat.eSend(image);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\9.jpg");
            formChat.eSend(image);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\10.jpg");
            formChat.eSend(image);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\11.jpg");
            formChat.eSend(image);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\12.jpg");
            formChat.eSend(image);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\13.jpg");
            formChat.eSend(image);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\14.jpg");
            formChat.eSend(image);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            Image image = Image.FromFile("...\\...\\Resources\\15.jpg");
            formChat.eSend(image);
        }
    }
}
