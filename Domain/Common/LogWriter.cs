﻿using log4net;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// General class for logging.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remark>
    /// Class has a static instance to the logmanager's logger with the T type. It has a performance effect when you create 
    /// more separate logwriters of the same type. 
    /// </remark>
    public sealed class LogWriter<T> : ILogWriter
    {
        private static ILog _log = LogManager.GetLogger(typeof(T));

        /// <summary>
        /// Sets the log explicitly.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <remarks>
        /// Used for test
        /// </remarks>
        public static void SetExplicitLog(ILog log)
        {
            _log = log;
        }

        public ILog Log
        {
            get { return _log; }
        }

        public void LogInfo(string message)
        {
            _log.Info(message);
        }
    }
}
