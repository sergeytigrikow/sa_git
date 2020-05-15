using System;
using System.IO;
using System.Net;
using CommonClassLib;

namespace ServerClasses.Analyzers
{
    public enum AnalyzeResult
    {
        WaitingForUrl,
        WaitingForAnalyze,
        Done,
        NotCriticalError,
        CriticalError,
        Analyzing
    }

    public abstract class Analyzer
    {
        private String _siteUrl;

        public AnalyzeResult Status { get; protected set; }
        public RequestType AnalyzerType;
        public String SiteUrl
        {
            get { return _siteUrl; }
            set
            {
                _siteUrl = value;
                SetUrlToParse();
                Status = AnalyzeResult.WaitingForAnalyze;
            }
        }

        public String Result { get; protected set; }

        protected String UrlToParse;
        protected int Retries = 0;

        abstract protected bool ParseInfo();
        abstract protected void SetUrlToParse();

        public void Proceed()
        {
            if (ParseInfo()) Status = AnalyzeResult.Done;
        }

        protected Analyzer(String url, RequestType type)
        {
            AnalyzerType = type;
            SiteUrl = url;
            Status = AnalyzeResult.WaitingForAnalyze;
        }

        protected Analyzer(RequestType type) 
        { 
            AnalyzerType = type;
            Status = AnalyzeResult.WaitingForUrl;
        }

        protected bool IsReady()
        {
            if (Status == AnalyzeResult.WaitingForAnalyze) return true;
            if (Status == AnalyzeResult.NotCriticalError)
            {
                Retries++;
                Status = AnalyzeResult.WaitingForAnalyze;
                return true;
            }
            return false;
        }

        protected string LoadParsingPage()
        {
            WebClient webClient = new WebClient();
            webClient.Headers.Add("User-Agent: Other");

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            string sDocument = string.Empty;
            using (Stream stream = webClient.OpenRead(UrlToParse))
            {
                var streamReader = new StreamReader(stream);
                sDocument = streamReader.ReadToEnd();
            }

            return sDocument;
        }

    }
}
