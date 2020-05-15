using CommonClassLib;
using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace ServerClasses.Analyzers
{
    public class LinksAnalyzer : Analyzer
    {
        public LinksAnalyzer(String url, RequestType type) : base(url, type) { }
        public LinksAnalyzer(RequestType type) : base(type) { }

        protected override bool ParseInfo()
        {
            if (!IsReady()) return false;
            Status = AnalyzeResult.Analyzing;

            try
            {
                String sDocument = LoadParsingPage();
                var doc = new HtmlDocument();
                doc.Load(new StringReader(sDocument));

                var links = doc.DocumentNode.SelectNodes("//a[@href]");
                if (links != null)
                {
                    var intern = links.Where(x =>
                        {
                            String href = x.GetAttributeValue("href", String.Empty);
                            return (href != String.Empty) && (href.Contains(SiteUrl) || href.StartsWith("/"));
                        }).ToList();
                    var external = links.Where(x => !intern.Contains(x)).ToList();
                    int closedFromGoogle = external.Where(x =>
                        {
                            String rel = x.GetAttributeValue("rel", string.Empty);
                            return (rel.ToLower() == "nofollow");
                        }).Count();
                    Result = String.Format("{0};{1}({2})", intern.Count, external.Count, closedFromGoogle);
                }
                else Result = String.Empty;
                return true;
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
                Result = String.Empty;
                Status = AnalyzeResult.NotCriticalError;
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Status = AnalyzeResult.CriticalError;
                Result = String.Empty;
                return false;
            }
        }

        protected override void SetUrlToParse()
        {
            UrlToParse = "https://" + SiteUrl;
        }
    }
}
