using Microsoft.Extensions.Configuration;
using MyTracker.Interfaces;

namespace MyTracker.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public ConfigurationService()
        {
        }

        public T GetSetting<T>(string key, T defaultValue) {

            var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("config.json", optional: true, reloadOnChange: true)
                    .Build();

            var value = config.GetSection(key).Get<T>();

            if (value == null)
                return defaultValue;

            return value;
        }
    }
}
