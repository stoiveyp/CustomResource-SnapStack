using Newtonsoft.Json;

namespace CustomResource_SnapStack
{
    public class SnapStackResourceProperties
    {
        [JsonProperty("table")]
        public string Table { get; set; }

        [JsonProperty("hash")]
        public string HashKey { get; set; }

        [JsonProperty("sort")]
        public string SortKey { get; set; }
    }
}