using Newtonsoft.Json;
using System;

namespace CommonClassLib.Requests
{
    public class CommandRequest : Request
    {
        [JsonProperty("Command")]
        public CommandType Command { get; set; }

        [JsonProperty("CommandInfo")]
        public String CommandInfo { get; set; }
    }
}
