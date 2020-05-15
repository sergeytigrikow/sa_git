using System;
using Newtonsoft.Json;
using CommonClassLib.Requests;
using CommonClassLib.Responces;

namespace CommonClassLib
{
    public class Serializator
    {
        public static String Serialize(Object req)
        {
            return JsonConvert.SerializeObject(req);
        }
        public static Request DeserializeRequest(String str)
        {
            if (str.Contains("Requests"))
                return JsonConvert.DeserializeObject<AnalyzeRequest>(str);
            if (str.Contains("Command")) 
                return JsonConvert.DeserializeObject<CommandRequest>(str);
            return null;
        }
        public static Responce DeserializeResponce(String str)
        {
            if (str.Contains("URL"))
                return JsonConvert.DeserializeObject<AnalyzeResponce>(str);
            if (str.Contains("Message"))
                return JsonConvert.DeserializeObject<CommandResponce>(str);
            return null;
        }
    }
}
