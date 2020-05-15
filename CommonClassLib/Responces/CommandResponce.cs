using Newtonsoft.Json;
using System;

namespace CommonClassLib.Responces
{
    public class CommandResponce : Responce
    {
        [JsonProperty("Message")]
        public String Message { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
