using Microsoft.Extensions.Logging;

namespace _3DNS
{
    /// <summary>
    /// Sends push notifications via the Ting endpoint on api.tzer0m.co.uk.
    /// </summary>
    internal static class TingClient
    {
        /// <summary>
        /// Send a Ting notification.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="apiKey">API key for api.tzer0m.co.uk</param>
        /// <param name="title">Notification title</param>
        /// <param name="body">Notification body</param>
        public static void Send(ILogger logger, string apiKey, string title, string body)
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

            string url = $"https://api.tzer0m.co.uk/Ting?title={Uri.EscapeDataString(title)}&body={Uri.EscapeDataString(body)}";
            HttpResponseMessage response = client.Send(new(HttpMethod.Get, url));
            try
            {
                response.EnsureSuccessStatusCode();
                logger.LogInformation("Ting notification sent");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Failed to send Ting notification");
            }
        }
    }
}