using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Net.Sockets;

namespace Choi_01
{
    public partial class FormChat : Form
    {
        private String ID, PW;  //loginform 에서 들어온데이터를 초기화 하기위해서 만든 변수. 

        public FormChat()
        {
            InitializeComponent();
        }

        private void tsExit_Click(object sender, EventArgs e)
        {
           this.Close();
        }

        private void FormChat_Load(object sender, EventArgs e)
        {
            //form이 load될때 . 
            //loginform의 id,pw,port 입력받은 값을 가져온다. 
            FormLogin LoginForm = new FormLogin();
            ID = LoginForm.ID;
            PW = LoginForm.PW;
            
        }

        private void FormChat_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormLogin LoginForm = new FormLogin();
            LoginForm.Visible = true;
        }
    }
}
