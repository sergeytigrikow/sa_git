using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace CommonClassLib.Responces
{
    public class AnalyzeResponce : Responce
    {
        [JsonProperty("URL")]
        public String Url { get; set; }
        [JsonProperty("Results")]
        public Dictionary<RequestType, String> Results = new Dictionary<RequestType, string>();
        [JsonProperty("LastUpdate")]
        public DateTime LastUpdate { get; set; }
        public override string ToString()
        {
            return Url;
        }

        public override bool Equals(object obj)
        {
            var toCompare = obj as AnalyzeResponce;
            if (toCompare == null) return false;

            if ((Url == toCompare.Url) && (LastUpdate == toCompare.LastUpdate))
            {
                return Results.All(result => (toCompare.Results.ContainsKey(result.Key)) && (toCompare.Results[result.Key] == result.Value));
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
