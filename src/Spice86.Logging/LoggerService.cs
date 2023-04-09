﻿using Spice86.Shared.Interfaces;

namespace Spice86.Logging;

using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;

public class LoggerService : ILoggerService {
    private const string LogFormat = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] [{IP:j}] {Message:lj}{NewLine}{Exception}";
    public LoggingLevelSwitch LogLevelSwitch { get; set; } = new(LogEventLevel.Warning);
    public bool AreLogsSilenced { get; set; }

    private ILogger _logger;

    private readonly ILogger _forcedLogger;

    private LoggerConfiguration _loggerConfiguration;

    public LoggerService() {
        _loggerConfiguration = CreateLoggerConfiguration();
        _logger = _loggerConfiguration
            .MinimumLevel.ControlledBy(LogLevelSwitch)
            .CreateLogger();
        _forcedLogger = CreateLoggerConfiguration().CreateLogger();
    }
    public LoggerConfiguration CreateLoggerConfiguration() {
        return new LoggerConfiguration()
            .Enrich.WithExceptionDetails()
            .WriteTo.Console(outputTemplate: LogFormat)
            .WriteTo.Debug(outputTemplate: LogFormat);
    }
    
    public LoggerConfiguration Override(string source, LogEventLevel minimumLevel) {
        _loggerConfiguration = _loggerConfiguration.MinimumLevel.Override(source, new LoggingLevelSwitch(minimumLevel));
        ((IDisposable)_logger).Dispose();
        _logger = _loggerConfiguration
            .MinimumLevel.ControlledBy(LogLevelSwitch)
            .CreateLogger();
        return _loggerConfiguration;
    }

#pragma warning disable Serilog004
    
    public void Forced(string messageTemplate, params object?[]? properties) {
        _forcedLogger.Debug(messageTemplate, properties);
    }
    
    public void Information(string messageTemplate, params object?[]? properties) {
        if (AreLogsSilenced) {
            return;
        }
        _logger.Information(messageTemplate, properties);
    }

    public void Warning(string message) {
        if (AreLogsSilenced) {
            return;
        }
        _logger.Warning(message);
    }
    
    public void Warning(Exception? e, string messageTemplate, params object?[]? properties) {
        if (AreLogsSilenced) {
            return;
        }
        _logger.Warning(e, messageTemplate, properties);
    }
    
    public void Error(Exception? e, string messageTemplate, params object?[]? properties) {
        if (AreLogsSilenced) {
            return;
        }
        _logger.Error(e, messageTemplate, properties);
    }
    
    public void Fatal(Exception? e, string messageTemplate, params object?[]? properties) {
        if (AreLogsSilenced) {
            return;
        }
        _logger.Fatal(e, messageTemplate, properties);
    }
    
    public void Warning(string messageTemplate, params object?[]? properties) {
        if (AreLogsSilenced) {
            return;
        }
        _logger.Warning(messageTemplate, properties);
    }
    
    public void Error(string messageTemplate, params object?[]? properties) {
        if (AreLogsSilenced) {
            return;
        }
        _logger.Error(messageTemplate, properties);
    }
    
    public void Fatal(string messageTemplate, params object?[]? properties) {
        if (AreLogsSilenced) {
            return;
        }
        _logger.Fatal(messageTemplate, properties);
    }
    
    public void Debug(string messageTemplate, params object?[]? properties) {
        if (AreLogsSilenced) {
            return;
        }
        _logger.Debug(messageTemplate, properties);
    }
    
    public void Verbose(string messageTemplate, params object?[]? properties) {
        if (AreLogsSilenced) {
            return;
        }
        _logger.Verbose(messageTemplate, properties);
    }
#pragma warning restore Serilog004

    public void Write(LogEvent logEvent) {
        _logger.Write(logEvent);
    }

    public bool IsEnabled(LogEventLevel level) {
        return _logger.IsEnabled(level);
    }
}
