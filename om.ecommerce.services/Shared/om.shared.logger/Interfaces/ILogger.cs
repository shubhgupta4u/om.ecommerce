using System;

namespace om.shared.logger.Interfaces
{
    public interface ILogger
    {
        Serilog.ILogger Log { get; }
        LogSettings LogSettings { get; }
        void LogError(Exception ex);
        void SetContext(string property, object value);
        void ResetContext();
    }
}
