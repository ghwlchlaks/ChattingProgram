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

namespace Choi_01
{
    public partial class FormLogin : Form
    {
        // Create a new instance of the Form2 class
        Create CreateForm = new Create();
        FormChat chatForm = new FormChat();
        public String ID, PW;
            
        public FormLogin()
        {
            InitializeComponent();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            // Show the settings form
            CreateForm.ShowDialog();  //모달형식  모달리스는 show();  
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            //입력받은 id,pw,port의 값을 초기화한다. public인이유는 다른 클래스에서 값을 사용하기위해서이다. 
            //상속을 이용해서 protected 로 바꿔서 사용해도 될것같다. 
            ID = txtId.Text;
            PW = txtPw.Text;       
        }

        private void FormLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                //id ip port pw 공백이면 포커스를 주고 다 입력했을경우 채팅창화면으로 들어감
                //(예정)id pw 틀렸을시 못들어가게 할거임..  
                //(예정)port 또는 ip 오류시 못들어가게 할거임
                if (txtId.Text == "")
                    txtId.Focus();
                else if (txtPw.Text == "")
                    txtPw.Focus();
                else if (txtPort.Text.ToString() == "")
                    txtPort.Focus();
                else if (txtIp.Text.ToString() == "")
                    txtIp.Focus();
                else
                {
                    this.Visible = false;
                    chatForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
              
            }
        }
    }
}
