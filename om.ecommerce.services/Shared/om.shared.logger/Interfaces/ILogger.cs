using om.shared.logger.models;
using System;

namespace om.shared.logger.Interfaces
{
    public interface ILogger
    {
        Serilog.ILogger Log { get; }
        LogSetting Setting { get; }
        void LogError(Exception ex);
        void SetContext(string property, object value);
        void ResetContext();
    }
}
