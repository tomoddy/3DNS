using Microsoft.Extensions.Logging;
using System.Configuration;

namespace _3DNS
{
    /// <summary>
    /// Config helper methods
    /// </summary>
    internal static class ConfigHelper
    {
        /// <summary>
        /// Gets value from configuration
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="key">Key</param>
        /// <returns>Config value</returns>
        /// <exception cref="KeyNotFoundException">Thrown if config value not present</exception>
        public static string GetValue(ILogger logger, string key)
        {
            string? value = ConfigurationManager.AppSettings[key];
            if (value is null)
            {
                logger.LogError("{key} is not set in the configuration.", key);
                throw new KeyNotFoundException($"{key} is not set in the configuration.");
            }

            logger.LogInformation("{key} loaded successfully ({value}).", key, value);
            return value;
        }
    }
}