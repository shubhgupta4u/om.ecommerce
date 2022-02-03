using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace om.shared.logger
{
    public class Logger: om.shared.logger.Interfaces.ILogger
    {
        private readonly LogSettings _logSettings;
        private static readonly object _padlock =new  object(); 
        private static Serilog.Core.Logger _logger;
        private static Dictionary<string, object> properties;
        public Logger()
        {
            if (Logger._logger == null)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                if (configuration != null)
                {
                    this._logSettings = new LogSettings();
                    this._logSettings.Sink = configuration.GetSection("LogSettings:Sink").Get<SinkLog>();
                    this._logSettings.AppInsightInstrumentationKey = configuration.GetSection("LogSettings:AppInsightInstrumentationKey")?.Value;
                    this._logSettings.ApplicationName = configuration.GetSection("LogSettings:ApplicationName")?.Value;
                    this._logSettings.LogFilePath = configuration.GetSection("LogSettings:LogFilePath")?.Value;
                }
            }
        }
        public LogSettings LogSettings
        {
            get
            {
                return this._logSettings;
            }
        }
        public Serilog.ILogger Log
        {
            get
            {
                if (Logger._logger == null)
                {
                    lock (_padlock)
                    {
                        if (Logger._logger == null)
                        {
                            Logger._logger = this.GetLogger();
                            Logger.properties = new Dictionary<string, object>();
                        }
                    }
                }
                return Logger._logger;
            }
        }
        public void ResetContext()
        {
            lock (_padlock)
            {
                Logger.properties = new Dictionary<string, object>();
                LogContext.Reset();
            }
        }
        public void SetContext(string property, object value)
        {
            if (Logger.properties !=null && !Logger.properties.ContainsKey(property))
            {
                lock (_padlock)
                {
                    Logger.properties.Add(property, value);
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
        private LoggerConfiguration GetLoggerConfiguration(string jsonConfigFile)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile(jsonConfigFile).Build();
           
            if (this._logSettings.Sink == SinkLog.File && !string.IsNullOrWhiteSpace(this._logSettings.ApplicationName))
            {
                var filePath = configuration["Serilog:WriteTo:0:Args:path"];
                if(!string.IsNullOrWhiteSpace(filePath) && filePath.Contains("{ApplicationName}"))
                {
                    configuration["Serilog:WriteTo:0:Args:path"] = filePath.Replace("{ApplicationName}", this._logSettings.ApplicationName);
                }
                
            }
            else if (this._logSettings.Sink == SinkLog.AzureBlobStorage && !string.IsNullOrWhiteSpace(this._logSettings.ApplicationName))
            {
                var filePath = configuration["Serilog:WriteTo:0:Args:storageFileName"];
                if (!string.IsNullOrWhiteSpace(filePath) && filePath.Contains("{ApplicationName}"))
                {
                    configuration["Serilog:WriteTo:0:Args:storageFileName"] = filePath.Replace("{ApplicationName}", this._logSettings.ApplicationName);
                }
            }
            else if (this._logSettings.Sink == SinkLog.MongoDb && !string.IsNullOrWhiteSpace(this._logSettings.ApplicationName))
            {
                var filePath = configuration["Serilog:WriteTo:0:Args:collectionName"];
                if (!string.IsNullOrWhiteSpace(filePath) && filePath.Contains("{ApplicationName}"))
                {
                    configuration["Serilog:WriteTo:0:Args:collectionName"] = filePath.Replace("{ApplicationName}", this._logSettings.ApplicationName);
                }
            }
            else if (this._logSettings.Sink == SinkLog.ApplicationInsight && !string.IsNullOrWhiteSpace(this._logSettings.ApplicationName))
            {
                var filePath = configuration["Serilog:Properties:Application"];
                if (!string.IsNullOrWhiteSpace(filePath) && filePath.Contains("{ApplicationName}"))
                {
                    configuration["Serilog:Properties:Application"] = filePath.Replace("{ApplicationName}", this._logSettings.ApplicationName);
                }
            }
            return new LoggerConfiguration().ReadFrom.Configuration(configuration);
        }
        private Serilog.Core.Logger GetLogger()
        {
            string jsonConfigFile = "logsettings.File.json";
            LoggerConfiguration loggerConfiguration = null;
            SinkLog sinkLog = SinkLog.File;
            if (this._logSettings != null)
            {
                sinkLog = this._logSettings.Sink;
            }
            switch (sinkLog)
            {
                case SinkLog.ApplicationInsight:
                    jsonConfigFile = "logsettings.ApplicationInsight.json";
                    loggerConfiguration = this.GetLoggerConfiguration(jsonConfigFile);

                    var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
                    telemetryConfiguration.InstrumentationKey = this._logSettings.AppInsightInstrumentationKey;
                    loggerConfiguration.WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces);

                    break;
                case SinkLog.AzureBlobStorage:
                    jsonConfigFile = "logsettings.AzureBlobStorage.json";
                    loggerConfiguration = this.GetLoggerConfiguration(jsonConfigFile);
                    break;
                case SinkLog.MongoDb:
                    jsonConfigFile = "logsettings.MongoDb.json";
                    loggerConfiguration = this.GetLoggerConfiguration(jsonConfigFile);
                    break;
                case SinkLog.File:
                    jsonConfigFile = "logsettings.File.json";
                    loggerConfiguration = this.GetLoggerConfiguration(jsonConfigFile);
                    break;
                default:
                    loggerConfiguration = this.GetLoggerConfiguration(jsonConfigFile);
                    break;

            };
            if (!string.IsNullOrWhiteSpace(this._logSettings.ApplicationName))
            {
                loggerConfiguration.Enrich.WithProperty("Application", this._logSettings.ApplicationName);
            }           
            
            return loggerConfiguration.CreateLogger();
        }
        #endregion
    }
}
