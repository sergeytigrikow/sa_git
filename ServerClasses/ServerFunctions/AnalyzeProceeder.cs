using System;
using System.Collections.Generic;
using System.Linq;
using ServerClasses.Analyzers;
using CommonClassLib;
using CommonClassLib.Responces;
using CommonClassLib.Requests;

namespace ServerClasses.ServerFunctions
{
    public class AnalyzeProceeder : Proceeder
    {
        private readonly List<Analyzer> Analyzers = new List<Analyzer>();
     

        public AnalyzeProceeder(QueueNode node)
        {
            RequestIProceed = node;
            Analyzers = new List<Analyzer>();
            CreateAnalyzers();
        }

        private void CreateAnalyzers()
        {
            var request = RequestIProceed.Request as AnalyzeRequest;
            if (request == null)
                return;

            foreach (RequestType type in request.Requests)
            {
                switch (type)
                {
                    case RequestType.CY:
                        Analyzers.Add(new CYAnalyzer(request.SiteUrl, type));
                        break;
                    case RequestType.PR:
                        Analyzers.Add(new PRAnalyzer(request.SiteUrl, type));
                        break;
                    case RequestType.Links:
                        Analyzers.Add(new LinksAnalyzer(request.SiteUrl, type));
                        break;
                    case RequestType.Customers:
                        Analyzers.Add(new CustomersAnalyzer(request.SiteUrl, type));
                        break;
                }
            }
        }

        private void MakeResponce()
        {
            var request = RequestIProceed.Request as AnalyzeRequest;
            var newresponce = new AnalyzeResponce();
            foreach (var a in Analyzers)
            {
                newresponce.Results.Add(a.AnalyzerType, a.Result);
            }
            newresponce.LastUpdate = DateTime.Now;

            if (request != null)
                newresponce.Url = request.SiteUrl;

            Responce = newresponce;
        }

        /* private AnalyzeResponce GetResponceFromDb()
        {
            var request = RequestIProceed.Request as AnalyzeRequest;
            var toCompare = request;
            if (toCompare != null)
            {
                var respFromDb = Dbprovider.GetReport(toCompare.SiteUrl);
                if (respFromDb == null) return null;
                if (toCompare.Requests.Any(item => !respFromDb.Results.ContainsKey(item)))
                {
                    return null;
                }
                if ((DateTime.Now - respFromDb.LastUpdate).TotalHours <= 1) return respFromDb;
                return null;
            }
            return null;
        } */


        public override void Proceed()
        {
            //Responce = GetResponceFromDb();
            //if (Responce == null)
            //{
                Responce = new AnalyzeResponce();
                foreach (Analyzer analyzer in Analyzers)
                {
                    analyzer.Proceed();
                }
                MakeResponce();
                //Dbprovider.SaveReport(Responce as AnalyzeResponce);
            //}
            OnProceedingFinished(new ResponceEventArgs(Responce, RequestIProceed));
        }



    }
}
