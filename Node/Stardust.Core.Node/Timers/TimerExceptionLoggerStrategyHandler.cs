using System;
using Microsoft.Extensions.Logging;

//using log4net;

namespace Stardust.Core.Node.Timers
{
    public class TimerExceptionLoggerStrategyHandler
    {
        private readonly TimeSpan _logInterval;
        private readonly ILogger _logger;
        private DateTime? _lastLoggedTime = null;
        
        public static readonly TimeSpan DefaultLogInterval = TimeSpan.FromMinutes(10);

        public TimerExceptionLoggerStrategyHandler(TimeSpan logInterval, Type logType)
        {
            _logInterval = logInterval;
            _logger = new LoggerFactory().CreateLogger(logType);
        }

        public void LogError(string logString, Exception exception = null)
        {
            if (!ShouldLog()) return;
            _logger.LogError(logString, exception);
            _lastLoggedTime = DateTime.UtcNow;
        }
        
        public void LogWarning(string logString, Exception exception = null)
        {
            if (!ShouldLog()) return;
            _logger.LogWarning(logString, exception);
            _lastLoggedTime = DateTime.UtcNow;
        }
        
        public void LogInfo(string logString, Exception exception = null)
        {
            if (!ShouldLog()) return;
            _logger.LogInformation(logString, exception);
            _lastLoggedTime = DateTime.UtcNow;
        }
        
        public void LogDebug(string logString, Exception exception = null)
        {
            if (!ShouldLog()) return;
            _logger.LogDebug(logString, exception);
            _lastLoggedTime = DateTime.UtcNow;
        }

        private bool ShouldLog()
        {
            return _lastLoggedTime == null || DateTime.UtcNow > _lastLoggedTime.Value.Add(_logInterval);
        }

        public void ResetLastLoggedTime(string message)
        {
            if (_lastLoggedTime == null) return;
            
            _lastLoggedTime = null;
            _logger.LogInformation(message);
        }
    }
}