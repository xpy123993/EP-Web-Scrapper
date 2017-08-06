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
    public partial class DetailDialog : Form
    {
        private Summary summary;


        public DetailDialog(Summary summary)
        {
            InitializeComponent();
            this.summary = summary;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DetailDialog_Load(object sender, EventArgs e)
        {
            title.Text = summary.title;
            releaseDate.Text = summary.release_date.ToString("yyyy-MM-dd");
            updateDate.Text = summary.update_date.ToString("yyyy-MM-dd");
            url.Text = summary.url;
            text.Text = summary.text;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", url.Text);
        }
    }
}
