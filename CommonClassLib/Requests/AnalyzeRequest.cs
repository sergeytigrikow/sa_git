using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CommonClassLib.Requests
{
    public class AnalyzeRequest : Request
    {
        [JsonProperty(PropertyName = "Requests")]
        public SortedSet<RequestType> Requests { get; set; }
        [JsonProperty(PropertyName = "SiteURL")]
        public String SiteUrl { get; set; }
        public AnalyzeRequest() 
        {
            Requests = new SortedSet<RequestType>();
        }
        public AnalyzeRequest(String site, SortedSet<RequestType> requests) 
        {
            Requests = requests;
            SiteUrl = site;
        }
        public override string ToString()
        {
            String result = Requests.Aggregate(String.Empty, (current, item) => current + (item.ToString() + '\n'));
            return String.Format("Сайт: {0}\nЗапросы:\n{1}\n", SiteUrl, result);
        }
    }
}
