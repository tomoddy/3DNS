using Newtonsoft.Json;

namespace _3DNS
{
    /// <summary>
    /// DNS record
    /// </summary>
    internal class Record
    {
        /// <summary>
        /// Data for the record (e.g. IP address for A record)
        /// </summary>
        [JsonProperty("data")]
        public string? Data { get; set; }

        /// <summary>
        /// Name of the record (e.g. "@" for root domain, "www" for www subdomain)
        /// </summary>
        [JsonProperty("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Time to live for the record in seconds (e.g. 600 for 10 minutes)
        /// </summary>
        [JsonProperty("ttl")]
        public int TTL { get; set; }

        /// <summary>
        /// Record type (e.g. A, CNAME)
        /// </summary>
        [JsonProperty("type")]
        public string? Type { get; set; }
    }
}