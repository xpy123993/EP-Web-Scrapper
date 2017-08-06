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
    public partial class SourceEditor : Form
    {

        public List<SourceConfig> sources;
        public ConfigHub configHub = ConfigHub.instance;
        public int selectedindex = -1;

        public SourceEditor()
        {
            InitializeComponent();
            sources = configHub.getSourceConfig();
        }

        private void refresh_list()
        {
            listBox1.Items.Clear();
            foreach (SourceConfig source in sources)
            {
                listBox1.Items.Add(source.source_name);
            }
            button5.Enabled = false;
        }

        private void SourceEditor_Load(object sender, EventArgs e)
        {
            refresh_list();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index < 0 || index >= sources.Count)
                return;
            sourceName.Text = sources[index].source_name;
            catalog_regex.Text = sources[index].catalog_regex;
            content_regex.Text = sources[index].content_regex;
            prefix_url.Text = sources[index].prefix_url;
            encoding.Text = sources[index].encoder;
            isUsing.Checked = sources[index].isUsing;
            listBox2.Items.Clear();
            foreach(String url in sources[index].catalog_url)
            {
                listBox2.Items.Add(url);
            }
            selectedindex = listBox1.SelectedIndex;
            button5.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SourceConfig sourceConfig = new SourceConfig();
            sourceConfig.source_name = sourceName.Text;
            sourceConfig.catalog_regex = catalog_regex.Text;
            sourceConfig.content_regex = content_regex.Text;
            sourceConfig.prefix_url = prefix_url.Text;
            sourceConfig.encoder = encoding.Text;
            sourceConfig.isUsing = isUsing.Checked;
            foreach (String url in listBox2.Items)
                sourceConfig.addCatalogURL(url);
            sources.Add(sourceConfig);
            configHub.setSourceConfig(sources);
            refresh_list();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            sources.RemoveAt(selectedindex);
            configHub.setSourceConfig(sources);
            selectedindex = -1;
            refresh_list();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (selectedindex < 0)
                return;
            sources[selectedindex].source_name = sourceName.Text;
            sources[selectedindex].catalog_regex = catalog_regex.Text;
            sources[selectedindex].content_regex = content_regex.Text;
            sources[selectedindex].prefix_url = prefix_url.Text;
            sources[selectedindex].encoder = encoding.Text;
            sources[selectedindex].isUsing = isUsing.Checked;
            sources[selectedindex].catalog_url.Clear();
            foreach (String url in listBox2.Items)
                sources[selectedindex].addCatalogURL(url);
            configHub.setSourceConfig(sources);
            refresh_list();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox2.Items.Add(textBox1.Text);
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex < 0 || listBox2.SelectedIndex > listBox2.Items.Count)
                return;
            listBox2.Items.RemoveAt(listBox2.SelectedIndex);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            configHub.initialize_source();
            sources = configHub.getSourceConfig();
            selectedindex = -1;
            refresh_list();
        }
    }
}
