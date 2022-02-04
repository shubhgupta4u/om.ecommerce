using Microsoft.Extensions.Configuration;

namespace om.shared.logger.models
{
    public class LogSetting
    {
        public string ApplicationName { get; set; }
        public SinkLog Sink { get; set; }
        public string AppInsightInstrumentationKey { get; set; }
        public IConfiguration SinkConfiguration { get; set; }
    }
}
