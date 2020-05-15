using System;
using System.Globalization;
using CommonClassLib;
using System.Net;
using HtmlAgilityPack;
using System.IO;

namespace ServerClasses.Analyzers
{
    public class PRAnalyzer : Analyzer
    {
        public PRAnalyzer(String url, RequestType type) : base(url, type) { }
        public PRAnalyzer(RequestType type) : base(type) { }

        protected override bool ParseInfo()
        {
            if (!IsReady())
                return false;

            try
            {
                var sDocument = LoadParsingPage();
                
                var doc = new HtmlDocument();
                doc.Load(new StringReader(sDocument));
                
                /*
                String hosts = doc.DocumentNode.SelectSingleNode(@"//body/table[4]/tr/tr/tr/tr/td[6]").InnerText;
                String views = doc.DocumentNode.SelectSingleNode(@"//body/table[4]/tr/tr/td[6]").InnerText;
                Result = String.Format("{0}:{1}", hosts, views);
                */

                string hosts = doc.DocumentNode.SelectSingleNode(@"//body/div").InnerText.Trim();
                string[] parts = hosts.Split(new string[] { "Worldwide" }, 2, StringSplitOptions.RemoveEmptyEntries);
                string prWorldwide = parts.Length >= 2 ? (parts[1] ?? string.Empty) : string.Empty;
                prWorldwide = prWorldwide.Trim();
                parts = prWorldwide.Split(new string[] { "\n" }, 2, StringSplitOptions.RemoveEmptyEntries);
                Result = parts.Length >= 1 ? (parts[0] ?? string.Empty) : string.Empty;
                //Result = GooglePR.MyPR(SiteUrl).ToString(CultureInfo.InvariantCulture);
                Status = AnalyzeResult.Done;
                return true;
            }
            catch (WebException ex)
            {
                Console.WriteLine("Не удалось соединиться с сервером!");
                Status = AnalyzeResult.NotCriticalError;
                return false;
            }
            catch (Exception)
            {
                Status = AnalyzeResult.CriticalError;
                return false;
            }
        }
       
        //protected override void SetUrlToParse() { }
        protected override void SetUrlToParse()
        {
            UrlToParse = String.Format(@"https://www.similarweb.com/website/{0}/", SiteUrl);
        }

        public override string ToString()
        {
            return String.Format("PR:{0};", Result);
        }
    }
}
