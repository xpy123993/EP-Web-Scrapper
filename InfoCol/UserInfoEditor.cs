using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InfoCol
{
    public partial class UserInfoEditor : Form
    {

        public List<AuthConfig> auths = ConfigHub.instance.getAuthConfig();

        public UserInfoEditor()
        {
            InitializeComponent();
        }

        private void refresh_list()
        {
            listBox1.Items.Clear();
            foreach(AuthConfig auth in auths)
            {
                listBox1.Items.Add(auth.username);
            }
            button3.Enabled = false;
        }

        private void UserInfoEditor_Load(object sender, EventArgs e)
        {
            refresh_list();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = auths[listBox1.SelectedIndex].username;
            textBox2.Text = auths[listBox1.SelectedIndex].password;
            button3.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0 || listBox1.SelectedIndex >= auths.Count) return;
            auths[listBox1.SelectedIndex].username = textBox1.Text;
            auths[listBox1.SelectedIndex].password = textBox2.Text;
            ConfigHub.instance.setAuthConfig(auths);
            refresh_list();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AuthConfig authConfig = new AuthConfig(textBox1.Text, textBox2.Text);
            auths.Add(authConfig);
            ConfigHub.instance.setAuthConfig(auths);
            refresh_list();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0 || listBox1.SelectedIndex >= auths.Count)
                return;
            auths.RemoveAt(listBox1.SelectedIndex);
            ConfigHub.instance.setAuthConfig(auths);
            refresh_list();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ConfigHub.instance.initialize_auth();
            auths = ConfigHub.instance.getAuthConfig();
            refresh_list();
        }
    }
}
