using System;
using System.Xml;
using System.Net;
using CommonClassLib;

namespace ServerClasses.Analyzers
{
    public class CYAnalyzer : Analyzer
    {
        public CYAnalyzer(String url, RequestType type) : base(url, type) { }
        public CYAnalyzer(RequestType type) : base(type) { }

        protected override bool ParseInfo()
        {
            if (!IsReady()) return false;
            Status = AnalyzeResult.Analyzing;

            var doc = new XmlDocument();
            try
            {
                doc.Load(UrlToParse);
                if (doc.DocumentElement != null)
                {
                    XmlNode tcy = doc.DocumentElement.SelectSingleNode("//tcy");
                    if (tcy != null)
                    {
                        if (tcy.Attributes != null) Result = tcy.Attributes.GetNamedItem("value").Value;
                        else throw new Exception("Не удалось пропарсить страницу;");
                    }
                    else throw new Exception("Не удалось пропарсить страницу;");
                } else throw new WebException("Не удалось загрузить страницу;");
                return true;
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
                Result = "-1";
                Status = AnalyzeResult.NotCriticalError;
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Status = AnalyzeResult.CriticalError;
                Result = "-1";
                return false;
            }
        }
        protected override void SetUrlToParse()
        {
            UrlToParse = "http://bar-navig.yandex.ru/u?ver=2&show=32&url=http://" + SiteUrl;
        }

        public override string ToString()
        {
            return String.Format("CY:{0};", Result);
        }

    }
}
