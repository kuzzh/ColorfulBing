
using Newtonsoft.Json;

namespace ColorfulBing.Model {

    public sealed class JBData {
        [JsonProperty("images")]
        public JImage[] Images { get; set; }
    }


    public sealed class JImage {
        [JsonProperty("startdate")]
        public string StartDate { get; set; }
        [JsonProperty("fullstartdate")]
        public string FullStartDate { get; set; }
        [JsonProperty("enddate")]
        public string EndDate { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("urlbase")]
        public string Urlbase { get; set; }
        [JsonProperty("copyright")]
        public string Copyright { get; set; }
        [JsonProperty("copyrightlink")]
        public string Copyrightlink { get; set; }
        [JsonProperty("quiz")]
        public string Quiz { get; set; }
        [JsonProperty("wp")]
        public bool WP { get; set; }
        [JsonProperty("hsh")]
        public string Hsh { get; set; }
        [JsonProperty("drk")]
        public int Drk { get; set; }
        [JsonProperty("top")]
        public int Top { get; set; }
        [JsonProperty("bot")]
        public int Bot { get; set; }
    }
}