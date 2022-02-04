using Microsoft.Extensions.Configuration;

namespace om.shared.logger.helpers
{
    public class LogSinkHelper
    {
        public static IConfiguration GetSinkConfiguration(SinkLog sinkLog, string applicationName)
        {
            string jsonConfigFile = "logsettings.File.json";
            string jsonPath = "Serilog:WriteTo:0:Args:path";
            switch (sinkLog)
            {
                case SinkLog.ApplicationInsight:
                    jsonConfigFile = "logsettings.ApplicationInsight.json";
                    jsonPath = "Serilog:Properties:Application";
                    break;
                case SinkLog.AzureBlobStorage:
                    jsonConfigFile = "logsettings.AzureBlobStorage.json";
                    jsonPath = "Serilog:WriteTo:0:Args:storageFileName";
                    break;
                case SinkLog.MongoDb:
                    jsonConfigFile = "logsettings.MongoDb.json";
                    jsonPath = "Serilog:WriteTo:0:Args:collectionName";
                    break;
                case SinkLog.File:
                    jsonConfigFile = "logsettings.File.json";
                    jsonPath = "Serilog:WriteTo:0:Args:path";
                    break;
            };
            var configuration = new ConfigurationBuilder().AddJsonFile(System.IO.Path.Combine("Settings", jsonConfigFile)).Build();
            UpdateApplicationName(configuration, jsonPath, applicationName);
            return configuration;

        }
        private static void UpdateApplicationName(IConfiguration configuration,string jsonPath, string applicationName)
        {
            var filePath = configuration[jsonPath];
            if (!string.IsNullOrWhiteSpace(filePath) && filePath.Contains("{ApplicationName}"))
            {
                configuration[jsonPath] = filePath.Replace("{ApplicationName}", applicationName);
            }
        }       
    }
}
