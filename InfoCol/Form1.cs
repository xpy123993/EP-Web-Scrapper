using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace InfoCol
{
    
    public partial class Form1 : Form
    {

        private ConfigHub configHub = ConfigHub.instance;

        public void verify()
        {
            if(configHub.verify(textBox1.Text, textBox2.Text))
            {
                //success
                MainFrame mf = new MainFrame();
                mf.Owner = this;
                this.Hide();
                mf.ShowDialog();
                this.Dispose();
            }
            else
            {
                //failed
                MessageBox.Show("用户名/密码错误");
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            verify();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*
            MainFrame mf = new MainFrame();
            mf.Owner = this;
            this.Hide();
            mf.ShowDialog();
            this.Dispose();*/
        }
    }
}
