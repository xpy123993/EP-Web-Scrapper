using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace InfoCol
{

    interface NewsProvider
    {
        List<Summary> update();
    }

    class NetUtils
    {

        public static String download(String url, String encoding, int retryTimes = 0)
        {
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.GetEncoding(encoding);
            string content = "";
            if (url == "") return "";
            try
            {
                content = webClient.DownloadString(url);
            }
            catch (Exception)
            {
                if(retryTimes > 3)
                    content = "获取失败";
                else
                {
                    Thread.Sleep(200);
                    download(url, encoding, retryTimes + 1);
                }
                    
            }
            finally
            {
                webClient.Dispose();
            }
            return content;
        }

    }

    class NormalProvdier: NewsProvider
    {
        public SourceConfig sourceConfig;
        public DateTime lastUpdateDate;
        public Boolean isTerminated = false;


        public NormalProvdier(SourceConfig sourceConfig, DateTime lastUpdateDate)
        {
            this.sourceConfig = sourceConfig;
            this.lastUpdateDate = lastUpdateDate;
        }

        public string getText(String url)
        {
            String webContent = NetUtils.download(url, sourceConfig.encoder);
            webContent = webContent.Replace("<BR>", "\n");
            webContent = webContent.Replace("<BR/>", "\n");
            webContent = webContent.Replace("<br>", "\n");
            webContent = webContent.Replace("<br />", "\n");
            webContent = webContent.Replace("&nbsp;", "");
            webContent = webContent.Replace("&ldquo;", "\"");
            webContent = webContent.Replace("&rdquo;", "\"");
            webContent = webContent.Replace("\t", "");
            String regexStr = sourceConfig.content_regex;
            String replaceRegex = @"<[^>]+>";
            String replaceRegex2 = @"\s{2, }";
            String text = Regex.Match(webContent, regexStr).Groups[0].ToString();
            text = Regex.Replace(text, replaceRegex2, " ");
            text = Regex.Replace(text, replaceRegex, "");
            text = text.Replace("\n", "\r\n");
            return text;
        }

        public List<Summary> getCatalog(String url)
        {
            if (isTerminated) return new List<Summary>();
            Console.WriteLine("loading " + url);
            String RegexStr = sourceConfig.catalog_regex;
            List<Summary> summarys = new List<Summary>();

            String webContent = NetUtils.download(url, sourceConfig.encoder);
            MatchCollection mc = Regex.Matches(webContent, RegexStr);

            foreach (Match m in mc)
            {
                Summary summary = new Summary();
                summary.url = sourceConfig.prefix_url + m.Groups["url"].ToString();
                summary.title = m.Groups["title"].ToString();
                summary.release_date = DateTime.Parse(m.Groups["time"].ToString());
                summary.source_name = sourceConfig.source_name;
                summary.text = getText(summary.url);
                
                if (summary.release_date.CompareTo(lastUpdateDate) < 0)
                    break;
                
                summarys.Add(summary);
            }

            return summarys;
        }

        public List<Summary> update()
        {
            List<Summary> summarys = new List<Summary>();
            foreach (string url in sourceConfig.catalog_url)
                summarys.AddRange(getCatalog(url));
                
            return summarys;
        }
    }
}
