namespace om.shared.logger
{
    public class LogSettings
    {
        public SinkLog Sink { get; set; }
        public string AppInsightInstrumentationKey { get; set; }
        public string ApplicationName { get; set; }
        public string LogFilePath { get; set; }
    }
}
