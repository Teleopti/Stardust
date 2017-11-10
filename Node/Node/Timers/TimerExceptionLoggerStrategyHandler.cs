using System;
using log4net;

namespace Stardust.Node.Timers
{
    public class TimerExceptionLoggerStrategyHandler
    {
        private readonly TimeSpan _logInterval;
        private readonly ILog _logger;
        private DateTime? _lastLoggedTime = null;
        
        public static readonly TimeSpan DefaultLogInterval = TimeSpan.FromMinutes(10);

        public TimerExceptionLoggerStrategyHandler(TimeSpan logInterval, Type logType)
        {
            _logInterval = logInterval;
            _logger = LogManager.GetLogger(logType);
        }

        public void LogError(string logString, Exception exception = null)
        {
            if (!ShouldLog()) return;
            _logger.Error(logString, exception);
            _lastLoggedTime = DateTime.UtcNow;
        }
        
        public void LogWarning(string logString, Exception exception = null)
        {
            if (!ShouldLog()) return;
            _logger.Warn(logString, exception);
            _lastLoggedTime = DateTime.UtcNow;
        }
        
        public void LogInfo(string logString, Exception exception = null)
        {
            if (!ShouldLog()) return;
            _logger.Info(logString, exception);
            _lastLoggedTime = DateTime.UtcNow;
        }
        
        public void LogDebug(string logString, Exception exception = null)
        {
            if (!ShouldLog()) return;
            _logger.Debug(logString, exception);
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
            _logger.Info(message);
        }
    }
}