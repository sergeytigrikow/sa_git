using Newtonsoft.Json;

namespace CommonClassLib.Responces
{
    public abstract class Responce
    {
        [JsonProperty("Result")]
        public bool Result { get; set; }
    }
}
