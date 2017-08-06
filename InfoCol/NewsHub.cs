using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Collections;
using System.Data.SqlClient;
using System.Data;

namespace InfoCol
{
    public class Summary
    {
        public int id = -1;
        public String title = "Unkown Title";
        public DateTime release_date = DateTime.Today;
        public DateTime update_date = DateTime.Today;
        public String source_name = "Unknown Source";
        public String text = "";
        public String url = "";
    }

    public class SummaryCompareMethod
    {
        public int compareByTitle(Summary a, Summary b)
        {
            return a.title.CompareTo(b.title);
        }

        public int compareByReleaseTime(Summary a, Summary b)
        {
            return -a.release_date.CompareTo(b.release_date);
        }

        public int compareBySourceName(Summary a, Summary b)
        {
            return a.source_name.CompareTo(b.source_name);
        }
    }

    public interface NewsHub
    {
        List<Summary> loadNews();
        void storeNews(List<Summary> data);
    }

    public class FileHub : NewsHub
    {
        public String data_file = "source.txt";

        private DateTime lastUpdateDate = new DateTime(2016, 1, 1);
        public DateTime getLastUpdateDate()
        {
            return lastUpdateDate;
        }
        public void setLastUpdateDate(DateTime dateTime)
        {
            this.lastUpdateDate = dateTime;
        }

        public List<Summary> loadNews()
        {
            List<Summary> summarys = new List<Summary>();
            if (!File.Exists(data_file)) return summarys;
            XmlDocument doc = new XmlDocument();
            doc.Load(data_file);
            XmlNodeList xnl = doc.ChildNodes;
            XmlNodeList nodes = xnl[0].ChildNodes;
            foreach (XmlNode node in nodes)
            {
                XmlNodeList attr = node.ChildNodes;
                Summary summary = new Summary();
                summary.title = attr[0].InnerText;
                summary.release_date = DateTime.FromBinary(long.Parse(attr[1].InnerText));
                summary.update_date = DateTime.FromBinary(long.Parse(attr[2].InnerText));
                if (summary.update_date.CompareTo(lastUpdateDate) > 0)
                    lastUpdateDate = summary.update_date;
                summary.source_name = attr[3].InnerText;
                summary.text = attr[4].InnerText;
                summary.url = attr[5].InnerText;
                summarys.Add(summary);
            }
            return summarys;
        }

        public void storeNews(List<Summary> data)
        {

            XmlDocument doc = new XmlDocument();
            XmlElement summaryNode, root, temp;

            root = doc.CreateElement("News");
            foreach (Summary summary in data)
            {
                summaryNode = doc.CreateElement("Info");

                temp = doc.CreateElement("Title");
                temp.InnerText = summary.title;
                summaryNode.AppendChild(temp);

                temp = doc.CreateElement("ReleaseDate");
                temp.InnerText = summary.release_date.ToBinary().ToString();
                summaryNode.AppendChild(temp);

                temp = doc.CreateElement("UpdateDate");
                temp.InnerText = summary.update_date.ToBinary().ToString();
                summaryNode.AppendChild(temp);

                temp = doc.CreateElement("SourceName");
                temp.InnerText = summary.source_name;
                summaryNode.AppendChild(temp);

                temp = doc.CreateElement("Text");
                temp.InnerText = summary.text;
                summaryNode.AppendChild(temp);

                temp = doc.CreateElement("URL");
                temp.InnerText = summary.url;
                summaryNode.AppendChild(temp);

                root.AppendChild(summaryNode);
            }
            doc.AppendChild(root);
            doc.Save(data_file);
        }

    }

    public class DatabaseImpl: NewsHub, IDisposable
    {

        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + Directory.GetCurrentDirectory() + @"\download_news.mdf;Integrated Security=True;Connect Timeout=30";

        public SqlConnection connection = null;
        public int home_max_items = 100;

        public static DatabaseImpl instance = new DatabaseImpl();

        DatabaseImpl() { initialize(); }
         ~DatabaseImpl() { finalize(); }

        private void initialize()
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
            create_table();
        }

        private void finalize()
        {
            if(connection != null && connection.State == ConnectionState.Connecting)
                connection.Close();
            connection = null;
        }

        //create/drop table
        private void execute_sql(string cmd)
        {
            if (connection != null)
            {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = cmd;
                command.ExecuteNonQuery();
                
            }
            else
            {
                Console.WriteLine("Error: Execute SQL command before connection established.");
            }
        }

        private List<Summary> summary_execute(SqlCommand cmd)
        {
            List<Summary> summarys = new List<Summary>();
            SqlDataAdapter adapter = new SqlDataAdapter();

            adapter.SelectCommand = cmd;
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            DataTable dt = ds.Tables[0];
            foreach (DataRow r in dt.Rows)
            {
                Summary summary = new Summary();
                summary.title = r["title"].ToString();
                summary.release_date = (DateTime)r["releasedate"];
                summary.update_date = (DateTime)r["updatedate"];
                summary.source_name = r["sourcename"].ToString();
                summary.text = r["text"].ToString();
                summary.url = r["url"].ToString();
                summarys.Add(summary);
            }
            return summarys;
        }

        private List<Summary> summary_execute(string cmd)
        {
            List<Summary> summarys = new List<Summary>();
            SqlDataAdapter adapter = new SqlDataAdapter(cmd, connection);
            DataSet ds = new DataSet();
            
            adapter.Fill(ds);
            if (ds.Tables.Count == 0)
                return summarys;
            DataTable dt = ds.Tables[0];
            foreach (DataRow r in dt.Rows)
            {
                Summary summary = new Summary();
                summary.id = (int)r["id"];
                summary.title = r["title"].ToString();
                summary.release_date = (DateTime)r["releasedate"];
                summary.update_date = (DateTime)r["updatedate"];
                summary.source_name = r["sourcename"].ToString();
                summary.text = r["text"].ToString();
                summary.url = r["url"].ToString();
                summarys.Add(summary);
            }
            return summarys;
        }

        public void create_table()
        {
            /*
             * Data Structure
             * 
             * ID, TITLE, RELEASEDATE, UPDATEDATE, SOURCENAME, TEXT, URL
             * 
             * CREATE TABLE [dbo].[news]
                (
	                [id] INT NOT NULL PRIMARY KEY IDENTITY, 
                    [title] NVARCHAR(128) NOT NULL, 
                    [releasedate] DATE NOT NULL, 
                    [updatedate] DATE NOT NULL, 
                    [sourcename] NVARCHAR(50) NOT NULL, 
                    [text] NVARCHAR(MAX) NOT NULL, 
                    [url] NVARCHAR(256) NOT NULL
                )
             * 
             * */
            string create_sql = "if not exists(select name from sysobjects where name='news')create table [dbo].[news] ( \n";
            create_sql += "[id] INT NOT NULL PRIMARY KEY IDENTITY, \n";
            create_sql += "[title] NVARCHAR(128) NOT NULL, \n";
            create_sql += "[releasedate] DATE NOT NULL, \n";
            create_sql += "[updatedate] DATE NOT NULL, \n";
            create_sql += "[sourcename] NVARCHAR(50) NOT NULL, \n";
            create_sql += "[text] NVARCHAR(MAX) NOT NULL, \n";
            create_sql += "[url] NVARCHAR(256) NOT NULL) ";

            execute_sql(create_sql);
        }

        public void drop_table()
        {
            string sql = "drop table [dbo].[news]";

            execute_sql(sql);
        }


        public List<Summary> loadNews()
        {
            string sqlCmd = "SELECT top " + home_max_items + " * FROM [dbo].[news] order by releasedate desc";
            return summary_execute(sqlCmd);
        }

        public void clear()
        {
            string sql = "DELETE FROM [dbo].[news]";
            execute_sql(sql);
        }

        public void storeNews(List<Summary> data)
        {

            insertNews(data);
        }

        public void insertNews(List<Summary> data)
        {
            string sql = "INSERT INTO [dbo].[news] VALUES(@title, @releasedate, @updatedate, @sourcename, @text, @url)";
            DateTime newestDate = getNewestReleaseDate();
            foreach (Summary summary in data)
            {
                if (newsExist(summary.title, summary.release_date, summary.source_name))
                    continue;
                SqlCommand cmd = new SqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@title", summary.title);
                cmd.Parameters.AddWithValue("@releasedate", summary.release_date);
                cmd.Parameters.AddWithValue("@updatedate", summary.update_date);
                cmd.Parameters.AddWithValue("@sourcename", summary.source_name);
                cmd.Parameters.AddWithValue("@text", summary.text);
                cmd.Parameters.AddWithValue("@url", summary.url);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Summary> selectByPeriod(DateTime minDate, DateTime maxDate)
        {
            string sql = "SELECT * FROM [dbo].[news] where releasedate between '" + minDate.ToString("yyyy-MM-dd hh:mm:ss") + "' and '" + maxDate.ToString("yyyy-MM-dd hh:mm:ss") + "' " + " ORDER BY releasedate DESC";
            return summary_execute(sql);
        }

        public List<Summary> selectBySourceName(string sourcename)
        {
            string sql = "SELECT * FROM [dbo].[news] where sourcename = N'" + sourcename + "'" + " ORDER BY releasedate DESC";
            return summary_execute(sql);
        }

        public List<Summary> selectByTitle(string title)
        {
            string sql = "SELECT * FROM [dbo].[news] where title like N'%" + title + "%' " + " ORDER BY releasedate DESC";
            return summary_execute(sql);
        }

        public DateTime getNewestReleaseDate()
        {
            string sql = "SELECT max(releasedate) as target FROM [dbo].[news]";
            SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            DataTable dt = ds.Tables[0];
            try
            {
                return (DateTime)dt.Rows[0]["target"];
            }
            catch (Exception)
            {
                return new DateTime(2016, 12, 1);
            }
        }

        public Boolean newsExist(string title, DateTime releaseDate, string sourceName)
        {
            string sql = "SELECT * FROM [dbo].[news] WHERE title = N'" + title + "'" +
                " AND convert(varchar, releasedate, 120) like '" + releaseDate.ToString("yyyy-MM-dd") + "%' " +
                " AND sourcename = N'" + sourceName + "'";
            return summary_execute(sql).Count > 0;
        }

        public void removeNews(int newsID)
        {
            string sql = "DELETE FROM [dbo].[news] where id = " + newsID;
            execute_sql(sql);
        }

        public void Dispose()
        {
            finalize();
        }
    }

    public class SourceConfig
    {
        public string source_name = "";
        public string catalog_regex = "";
        public string content_regex = "";
        public List<string> catalog_url = new List<string>();
        public String prefix_url = "";
        public String encoder = "";
        public bool isUsing = true;
        public void addCatalogURL(String url) { catalog_url.Add(url); }
    }

    public class AuthConfig
    {
        public string username = "";
        public string password = "";
        public AuthConfig(String username = "", String password = "")
        {
            this.username = username;
            this.password = password;
        }
    }

    public class ConfigHub
    {
        private XmlDocument doc = new XmlDocument();
        public const string DocFile = "config.xml";

        private List<AuthConfig> auths = new List<AuthConfig>();
        private List<SourceConfig> sources = new List<SourceConfig>();
        private DateTime lastUpdateDate = new DateTime(2016, 12, 1);

        public static ConfigHub instance = new ConfigHub();


        public DateTime getLastUpdateDate()
        {
            return lastUpdateDate;
        }

        public void setLastUpdateDate(DateTime dateTime)
        {
            lastUpdateDate = dateTime;
        }

        private ConfigHub()
        {
            if (!File.Exists(DocFile))
            {
                initialize_auth();
                initialize_source();
                store();
            }
            else
            {
                load();
            }
        }

        public void initialize_source()
        {
            sources.Clear();

            SourceConfig sourceConfig = new SourceConfig();
            sourceConfig.source_name = "国家自然科学基金委";
            sourceConfig.catalog_regex = @"href=""(?<url>[^""]+?)"" id=""[\S]+"" title=""(?<title>[^""]+?)"" target=""_blank"">[\S]+</a></span><span class=""fr"">(?<time>[^<]+?)</span>";
            sourceConfig.content_regex = @"<!--ContentStart-->(?<content>((.|\r|\n)*))<!--ContentEnd-->";
            sourceConfig.addCatalogURL("http://www.nsfc.gov.cn/publish/portal0/tab87/module467/page1.htm");
            sourceConfig.addCatalogURL("http://www.nsfc.gov.cn/publish/portal0/tab87/module467/page2.htm");
            sourceConfig.addCatalogURL("http://www.nsfc.gov.cn/publish/portal0/tab87/module467/page3.htm");
            sourceConfig.addCatalogURL("http://www.nsfc.gov.cn/publish/portal0/tab87/module467/page4.htm");
            sourceConfig.addCatalogURL("http://www.nsfc.gov.cn/publish/portal0/tab87/module467/page5.htm");
            sourceConfig.prefix_url = "http://www.nsfc.gov.cn";
            sourceConfig.encoder = "utf-8";
            sources.Add(sourceConfig);
            //<td class="STYLE30"><a href="./201704/t20170401_132280.htm" target="_blank" class=STYLE30>关于报送完善财政科研项目资金管理等政策落实情况的通知</a>(2017-04-01)

            sourceConfig = new SourceConfig();
            sourceConfig.source_name = "国家科学技术部";
            sourceConfig.catalog_regex = @"href=""(?<url>[^""]+?)"" target=""_blank"" class=STYLE30>(?<title>[^<]+?)</a>\((?<time>[^\)]+?)\)";
            sourceConfig.content_regex = @"<meta name=""ContentStart""/>(?<content>((.|\r|\n)*))<meta name=""ContentEnd""/>";
            sourceConfig.addCatalogURL("http://www.most.gov.cn/tztg/index.htm");
            sourceConfig.addCatalogURL("http://www.most.gov.cn/tztg/index_1.htm");
            sourceConfig.addCatalogURL("http://www.most.gov.cn/tztg/index_2.htm");
            sourceConfig.addCatalogURL("http://www.most.gov.cn/tztg/index_3.htm");
            sourceConfig.addCatalogURL("http://www.most.gov.cn/tztg/index_4.htm");
            sourceConfig.prefix_url = "http://www.most.gov.cn/tztg/";
            sourceConfig.encoder = "gb2312";
            sources.Add(sourceConfig);

            //<li><span class="neimk2time">2017-03-23</span><a href="http://www.lninfo.gov.cn/index.php?m=content&c=index&a=show&catid=19&id=88803" 
            //target ="_blank" style="font-weight:bold;" >关于征求《可持续发展实验区创新驱动发展评价暂行办法》和评价指标体系意见的通知</a></li>
            sourceConfig = new SourceConfig();
            sourceConfig.source_name = "辽宁省科学技术厅";
            sourceConfig.catalog_regex = @"<li><span class=[^>]+>(?<time>[^<]+?)</span><a href=""(?<url>[^""]+?)"" [^>]+>(?<title>[^<]+?)</a></li>";
            sourceConfig.content_regex = @"<!--newsbody begin-->(?<content>((.|\r|\n)*))<!--newsbody end-->";
            sourceConfig.addCatalogURL("http://www.lninfo.gov.cn/index.php?m=content&c=index&a=lists&catid=19");
            sourceConfig.addCatalogURL("http://www.lninfo.gov.cn/index.php?m=content&c=index&a=lists&catid=19&page=2");
            sourceConfig.addCatalogURL("http://www.lninfo.gov.cn/index.php?m=content&c=index&a=lists&catid=19&page=3");
            sourceConfig.addCatalogURL("http://www.lninfo.gov.cn/index.php?m=content&c=index&a=lists&catid=19&page=4");
            sourceConfig.addCatalogURL("http://www.lninfo.gov.cn/index.php?m=content&c=index&a=lists&catid=19&page=5");
            sourceConfig.prefix_url = "";
            sourceConfig.encoder = "gb2312";
            sources.Add(sourceConfig);
        }

        public void initialize_auth()
        {
            auths.Clear();
            auths.Add(new AuthConfig("admin", "admin"));
        }

        public void load()
        {
            doc = new XmlDocument();
            doc.Load(DocFile);
            XmlElement root = doc.DocumentElement;

            auths.Clear();
            sources.Clear();

            Hashtable table = new Hashtable();

            foreach (XmlElement block in root.ChildNodes)
            {
                if (block.Name == "AuthInfo")
                {
                    foreach (XmlElement authInfo in block.ChildNodes)
                    {
                        table.Clear();
                        table.Add("Username", "");
                        table.Add("Password", "");
                        foreach (XmlElement element in authInfo.ChildNodes)
                        {
                            if (table.ContainsKey(element.Name))
                                table[element.Name] = element.InnerText;
                        }
                        AuthConfig authConfig = new AuthConfig(table["Username"].ToString(), table["Password"].ToString());
                        auths.Add(authConfig);
                    }
                }
                if (block.Name == "SourceInfo")
                {
                    foreach (XmlElement sourceInfo in block.ChildNodes)
                    {
                        table.Clear();
                        table.Add("SourceName", "");
                        table.Add("CatalogRegex", "");
                        table.Add("ContentRegex", "");
                        table.Add("PrefixURL", "");
                        table.Add("Encoding", "");
                        table.Add("IsUsing", "false");
                        table.Add("CatalogURL", new List<string>());
                        foreach (XmlElement element in sourceInfo.ChildNodes)
                        {
                            if (table.ContainsKey(element.Name))
                            {
                                if (element.Name.Equals("CatalogURL"))
                                    ((List<string>)table["CatalogURL"]).Add(element.InnerText);
                                else
                                    table[element.Name] = element.InnerText;
                            }
                        }
                        SourceConfig sourceConfig = new SourceConfig();
                        sourceConfig.source_name = table["SourceName"].ToString();
                        sourceConfig.catalog_regex = table["CatalogRegex"].ToString();
                        sourceConfig.content_regex = table["ContentRegex"].ToString();
                        sourceConfig.prefix_url = table["PrefixURL"].ToString();
                        sourceConfig.encoder = table["Encoding"].ToString();
                        sourceConfig.isUsing = bool.Parse(table["IsUsing"].ToString());
                        sourceConfig.catalog_url = (List<string>)table["CatalogURL"];
                        sources.Add(sourceConfig);
                    }
                }
                if (block.Name.Equals("LastUpdateDate"))
                {
                    lastUpdateDate = DateTime.FromBinary(long.Parse(block.InnerText));
                }
            }
        }

        public void store()
        {
            doc = new XmlDocument();
            XmlElement root = doc.CreateElement("Configure");
            XmlElement auth_block = doc.CreateElement("AuthInfo");
            XmlElement source_block = doc.CreateElement("SourceInfo");
            XmlElement lastUpdateDate_block = doc.CreateElement("LastUpdateDate");
            lastUpdateDate_block.InnerText = lastUpdateDate.ToBinary().ToString();

            foreach (AuthConfig auth in auths)
            {
                XmlElement user = doc.CreateElement("User");
                XmlElement username = doc.CreateElement("Username");
                username.InnerText = auth.username;
                XmlElement password = doc.CreateElement("Password");
                password.InnerText = auth.password;
                user.AppendChild(username);
                user.AppendChild(password);
                auth_block.AppendChild(user);
            }

            foreach (SourceConfig source in sources)
            {
                XmlElement sourceXML = doc.CreateElement("Source");
                XmlElement temp = doc.CreateElement("SourceName");
                temp.InnerText = source.source_name;
                sourceXML.AppendChild(temp);

                temp = doc.CreateElement("CatalogRegex");
                temp.InnerText = source.catalog_regex;
                sourceXML.AppendChild(temp);

                temp = doc.CreateElement("ContentRegex");
                temp.InnerText = source.content_regex;
                sourceXML.AppendChild(temp);

                temp = doc.CreateElement("PrefixURL");
                temp.InnerText = source.prefix_url;
                sourceXML.AppendChild(temp);

                temp = doc.CreateElement("Encoding");
                temp.InnerText = source.encoder;
                sourceXML.AppendChild(temp);

                temp = doc.CreateElement("IsUsing");
                temp.InnerText = source.isUsing.ToString();
                sourceXML.AppendChild(temp);

                foreach (String url in source.catalog_url)
                {
                    temp = doc.CreateElement("CatalogURL");
                    temp.InnerText = url;
                    sourceXML.AppendChild(temp);
                }

                source_block.AppendChild(sourceXML);
            }

            root.AppendChild(auth_block);
            root.AppendChild(source_block);
            root.AppendChild(lastUpdateDate_block);

            doc.AppendChild(root);

            doc.Save(DocFile);
        }

        public Boolean verify(String username, String password)
        {
            foreach (AuthConfig auth in auths)
            {
                if (auth.username.Equals(username))
                    return auth.password.Equals(password);
            }
            return false;
        }

        public List<AuthConfig> getAuthConfig()
        {
            return auths;
        }

        public List<SourceConfig> getSourceConfig()
        {
            return sources;
        }

        public void setAuthConfig(List<AuthConfig> data)
        {
            auths = new List<AuthConfig>(data);
        }

        public void setSourceConfig(List<SourceConfig> data)
        {
            sources = new List<SourceConfig>(data);
        }
    }
}
