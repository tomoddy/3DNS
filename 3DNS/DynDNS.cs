using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace _3DNS
{
    /// <summary>
    /// Dyanamic DNS updater for GoDaddy domains
    /// </summary>
    internal class DynDNS
    {
        /// <summary>
        /// Update A record for specified domain to current public IP address if it has changed
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="domain">Domain</param>
        /// <param name="ip">Current public IP address</param>
        /// <param name="apiKey">Api key</param>
        /// <param name="apiSecret">Api secret</param>
        public static Outcome Run(ILogger logger, string domain, string ip, string apiKey, string apiSecret)
        {
            // Create HTTP client and request
            logger.LogInformation("Getting A record for {domain}", domain);
            HttpClient gcGetClient = new();
            HttpRequestMessage gdGetRequest = new(HttpMethod.Get, $"https://api.godaddy.com/v1/domains/{domain}/records/A");
            gdGetRequest.Headers.Add("Authorization", $"sso-key {apiKey}:{apiSecret}");

            // Send request and check for success
            HttpResponseMessage gdGetResponse = gcGetClient.Send(gdGetRequest);
            try
            {
                gdGetResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Failed to get A record for {domain}", domain);
                return Outcome.Failure;
            }
            logger.LogInformation("Successfully got A record for {domain}", domain);

            // Read response content
            List<Record>? records = JsonConvert.DeserializeObject<List<Record>>(gdGetResponse.Content.ReadAsStringAsync().Result);
            if (records is null)
            {
                logger.LogError("Failed to parse A record for {domain}", domain);
                return Outcome.Failure;
            }
            if (records.Count != 1)
            {
                logger.LogError("Unexpected number of A records for {domain} (expected 1): {count}", domain, records.Count);
                return Outcome.Failure;
            }

            // Check if record already has correct IP
            Record record = records[0];
            if (record.Data == ip)
            {
                logger.LogInformation("A record for {domain} is already up to date", domain);
                return Outcome.SuccessNoChange;
            }
            logger.LogWarning("IP address for {domain} has changed from {oldIp} to {newIp}", domain, record.Data, ip);

            // Create HTTP client and request
            logger.LogInformation("Updating A record for {domain} to {ip}", domain, ip);
            HttpClient gdPutClient = new();
            HttpRequestMessage gdPutRequest = new(HttpMethod.Put, $"https://api.godaddy.com/v1/domains/{domain}/records/A");
            gdPutRequest.Headers.Add("Authorization", $"sso-key {apiKey}:{apiSecret}");
            gdPutRequest.Content = new StringContent(JsonConvert.SerializeObject(new List<Record> { new() { Data = ip, Name = "@", TTL = 600, Type = "A" } }), null, "application/json");

            // Send request and check for success
            HttpResponseMessage gdPutResponse = gdPutClient.Send(gdPutRequest);
            try
            {
                gdPutResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Failed to update A record");
                return Outcome.Failure;
            }

            // Return success
            logger.LogInformation("Successfully updated A record");
            return Outcome.SuccessWithChange;
        }
    }
}