using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace InfoCol
{

    public partial class MainFrame : Form
    {
        private DatabaseImpl infoHub = DatabaseImpl.instance;
        private ConfigHub configHub = ConfigHub.instance;

        delegate void UpdateListViewCallback(List<Summary> data);

        private List<Summary> current_list = new List<Summary>();

        private void show_listview(List<Summary> data)
        {
            
            if (listView1.InvokeRequired)
            {
                while(!listView1.IsHandleCreated)
                {
                    if (listView1.Disposing || listView1.IsDisposed)
                        return;
                }
                UpdateListViewCallback u = new UpdateListViewCallback(show_listview);
                listView1.Invoke(u, new object[] { data });
            }
            else
            {
                listView1.BeginUpdate();
                listView1.Items.Clear();


                foreach (Summary summary in data)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = summary.title;
                    lvi.SubItems.Add(summary.release_date.ToString("yyyy-MM-dd"));
                    lvi.SubItems.Add(summary.source_name);
                    lvi.SubItems.Add(summary.update_date.ToString("yyyy-MM-dd"));
                    listView1.Items.Add(lvi);
                }
                listView1.EndUpdate();
                current_list = data;
            }
        }

        private void test_hub()
        {
            List<Summary> summarys = new List<Summary>();

            Summary summary;

            summary = new Summary();
            summary.title = "title-1";
            summary.release_date = DateTime.Today;
            summary.update_date = DateTime.Today;
            summary.source_name = "test source";
            summary.text = "text-1";
            summary.url = "no url";
            summarys.Add(summary);

            summary = new Summary();
            summary.title = "title-2";
            summary.release_date = DateTime.Today;
            summary.update_date = DateTime.Today;
            summary.source_name = "test source2";
            summary.text = "text-2";
            summary.url = "no url";
            summarys.Add(summary);

            infoHub.storeNews(summarys);
            summarys = infoHub.loadNews();

            show_listview(summarys);
        }

        public MainFrame()
        {
            InitializeComponent();
        }

        private void MainFrame_Load(object sender, EventArgs e)
        {
            List<Summary> summarys = infoHub.loadNews();
            show_listview(summarys);

            comboBox2.Items.Clear();
            foreach(SourceConfig sourceConfig in configHub.getSourceConfig())
            {
                comboBox2.Items.Add(sourceConfig.source_name);
            }
            comboBox2.Items.Add("ALL");
            comboBox2.SelectedIndex = comboBox2.Items.Count - 1;
            this.WindowState = FormWindowState.Maximized;
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void update_2()
        {
            List<Summary> data = new List<Summary>();
            
            toolStripStatusLabel1.Text = "更新中";

            List<SourceConfig> sourceConfigs = configHub.getSourceConfig();

            foreach (SourceConfig sourceConfig in sourceConfigs)
            {
                if (!sourceConfig.isUsing)
                    continue;
                toolStripStatusLabel1.Text = "正在更新源：" + sourceConfig.source_name;
                NormalProvdier np = new NormalProvdier(sourceConfig, configHub.getLastUpdateDate());
                data.AddRange(np.update());
            }

            infoHub.storeNews(data);
            show_listview(infoHub.loadNews());
            
            toolStripStatusLabel1.Text = "更新完毕";

            configHub.setLastUpdateDate(DateTime.Today);
            configHub.store();

        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = listView1.SelectedItems[0].Index;
            if (index < 0 || index >= current_list.Count)
                return;
            DetailDialog dd = new DetailDialog(current_list[index]);
            dd.Owner = this;
            dd.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            new Thread(update_2).Start();
        }

        private void update_filter_list()
        {
            comboBox2.Items.Clear();
            foreach (SourceConfig sourceConfig in configHub.getSourceConfig())
            {
                comboBox2.Items.Add(sourceConfig.source_name);
            }
            comboBox2.Items.Add("ALL");
            comboBox2.SelectedIndex = comboBox2.Items.Count - 1;
        }


        private void button2_Click_1(object sender, EventArgs e)
        {
            show_listview(infoHub.loadNews());
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            new Thread(update_2).Start();
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            SourceEditor se = new SourceEditor();
            se.Owner = this;
            se.ShowDialog();
            configHub.store();
            update_filter_list();
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            UserInfoEditor uie = new UserInfoEditor();
            uie.Owner = this;
            uie.ShowDialog();
            configHub.store();
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            if (MessageBox.Show("您将清空所有已下载内容", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                infoHub.clear();
                
                configHub.setLastUpdateDate(new DateTime(2016, 12, 1));
                configHub.store();
                show_listview(infoHub.loadNews());
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            List<Summary> matched = infoHub.selectByTitle(textBox1.Text);
            show_listview(matched);
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            List<Summary> matched = infoHub.selectByPeriod(dateTimePicker1.Value, dateTimePicker2.Value);
            show_listview(matched);
        }

        private void comboBox2_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            String sourceName = comboBox2.Text;
            if (sourceName.Equals("ALL"))
                show_listview(infoHub.loadNews());
            else
                show_listview(infoHub.selectBySourceName(sourceName));
        }

        private void MainFrame_SizeChanged(object sender, EventArgs e)
        {
            listView1.BeginUpdate();
            listView1.Columns[0].Width = listView1.Width * 350 / 800;
            listView1.Columns[1].Width = listView1.Width * 130 / 800;
            listView1.Columns[2].Width = listView1.Width * 150 / 800;
            listView1.Columns[3].Width = listView1.Width * 130 / 800;
            listView1.EndUpdate();
        }
    }
}
