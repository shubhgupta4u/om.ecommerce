using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;
using om.shared.logger.models;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace om.shared.logger
{
    public class Logger: om.shared.logger.Interfaces.ILogger
    {
        private readonly LogSetting _LogSetting;
        private readonly Serilog.Core.Logger _logger;
        private readonly static object _padlock = new object();
        private readonly Dictionary<string, object> properties;
        public Logger(IOptions<LogSetting> logSetting)
        {
            this._LogSetting = logSetting.Value;
            this._logger = this.GetLogger();
            this.properties = new Dictionary<string, object>();
        }
        public LogSetting Setting
        {
            get
            {
                return this._LogSetting;
            }
        }
        public Serilog.ILogger Log
        {
            get
            {
                return this._logger;
            }
        }
        public void ResetContext()
        {
            lock (_padlock)
            {
                this.properties.Clear();
                LogContext.Reset();
            }
        }
        public void SetContext(string property, object value)
        {
            if (this.properties !=null && !this.properties.ContainsKey(property))
            {
                lock (_padlock)
                {
                    this.properties.Add(property, value);
                    LogContext.PushProperty(property, value, true);
                }                
            }            
        }
        public void LogError(Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Error message:{ex.Message}");
            sb.Append($"Error stack trace:{ex.StackTrace}");
            this.Log.Error(sb.ToString());
        }
        #region Private Methods
        private Serilog.Core.Logger GetLogger()
        {
            LoggerConfiguration loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(this._LogSetting.SinkConfiguration);
            if (this._LogSetting.Sink == SinkLog.ApplicationInsight)
            {
                var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
                telemetryConfiguration.InstrumentationKey = this._LogSetting.AppInsightInstrumentationKey;
                loggerConfiguration.WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces);
            }
            
            if (!string.IsNullOrWhiteSpace(this._LogSetting.ApplicationName))
            {
                loggerConfiguration.Enrich.WithProperty("Application", this._LogSetting.ApplicationName);
            }           
            
            return loggerConfiguration.CreateLogger();
        }
        #endregion
    }
}
