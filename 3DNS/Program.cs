using _3DNS;
using Microsoft.Extensions.Logging;

/// <summary>
/// Entry point
/// </summary>
internal class Program
{
    /// <summary>
    /// Main method
    /// </summary>
    private static void Main()
    {
        // Create logger
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger logger = factory.CreateLogger("3DNS");

        // Load configuration
        string domain = ConfigHelper.GetValue(logger, "Domain");
        string apiKey = ConfigHelper.GetValue(logger, "ApiKey");
        string apiSecret = ConfigHelper.GetValue(logger, "ApiSecret");
    }
}