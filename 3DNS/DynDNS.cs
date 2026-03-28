using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

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
        /// <param name="apiKey">Api key</param>
        /// <param name="apiSecret">Api secret</param>
        public static void Run(ILogger logger, string domain, string apiKey, string apiSecret)
        {
            // Create HTTP client and request
            logger.LogInformation("Getting current public IP address");
            HttpClient ipClient = new();

            // Send request and check for success
            HttpResponseMessage ipResponse = ipClient.Send(new(HttpMethod.Get, "https://api.ipify.org"));
            try
            {
                ipResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Failed to get public IP");
                return;
            }
            logger.LogInformation("Public IP retrieved");

            // Read response content
            string ip = ipResponse.Content.ReadAsStringAsync().Result;
            if (!IPAddress.TryParse(ip, out _))
            {
                logger.LogError("Invalid IP address retrieved: {ip}", ip);
                return;
            }
            logger.LogInformation("Current public IP address: {ip}", ip);

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
                return;
            }
            logger.LogInformation("Successfully got A record for {domain}", domain);

            // Read response content
            List<Record>? records = JsonConvert.DeserializeObject<List<Record>>(gdGetResponse.Content.ReadAsStringAsync().Result);
            if (records is null)
            {
                logger.LogError("Failed to parse A record for {domain}", domain);
                return;
            }
            if (records.Count != 1)
            {
                logger.LogError("Unexpected number of A records for {domain} (expected 1): {count}", domain, records.Count);
                return;
            }

            // Check if record already has correct IP
            Record record = records[0];
            if (record.Data == ip)
            {
                logger.LogInformation("A record for {domain} is already up to date", domain);
                return;
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
                return;
            }
            logger.LogInformation("Successfully updated A record");
        }
    }
}