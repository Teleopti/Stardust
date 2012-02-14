using System;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;

namespace Teleopti.Logging.Core
{
    public class DatabaseTraceListener : CustomTraceListener
    {
        private readonly LogEntryInserter _inserter;
        private readonly string _connectionString;
        private const string ConnectionString = "MessageBroker";
        private bool disposed;

        public DatabaseTraceListener()
        {
            _connectionString = ConfigurationManager.AppSettings[ConnectionString]; 
            _inserter = new LogEntryInserter(_connectionString);
        }

        public override void Initialise(string fileName, bool rollover)
        {
            
        }

        public override void Initialise(string fileName, double rolloverInterval)
        {
            
        }

        public override void Write(string message)
        {
            QueueReader.QueueUserWorkItem(new WaitCallback(WriteMessage), message);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)")]
        public override void WriteLine(string message)
        {
            message = message.Replace(',', ' ');
            message = String.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Empty", message);
            QueueReader.QueueUserWorkItem(new WaitCallback(WriteLineMessage), message);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)")]
        public override void WriteLine(string message, string category)
        {
            message = message.Replace(',', ' ');
            message = String.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), category, message);
            QueueReader.QueueUserWorkItem(new WaitCallback(WriteLineMessage), message);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)")]
        public override void WriteEventLogEntry(string message, EventLogEntryType eventType)
        {
            message = message.Replace(',', ' ');
            message = String.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), eventType, message);
            LogItem item = new LogItem(message, eventType);
            QueueReader.QueueUserWorkItem(new WaitCallback(WriteEventLogEntry), item);
        }

        private void WriteMessage(object obj)
        {
            lock (LockObject)
            {
                string message = obj.ToString();
                LogEntry entry = new LogEntry(Guid.Empty, Process.GetCurrentProcess().Id, message, string.Empty, string.Empty, string.Empty, Environment.UserName, DateTime.Now);
                _inserter.Execute(entry);
            }
        }

        private void WriteLineMessage(object obj)
        {
            lock (LockObject)
            {
                string message = obj.ToString();
                LogEntry entry = new LogEntry(Guid.Empty, Process.GetCurrentProcess().Id, message, string.Empty, string.Empty, string.Empty, Environment.UserName, DateTime.Now);
                _inserter.Execute(entry);
            }
        }

        private void WriteEventLogEntry(object obj)
        {
            lock (LockObject)
            {
                LogItem item = (LogItem)obj;
                LogEntry entry = new LogEntry(Guid.Empty, Process.GetCurrentProcess().Id, item.Message, item.LogEntryType.ToString(), string.Empty, string.Empty, Environment.UserName, DateTime.Now);
                _inserter.Execute(entry);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    QueueReader.Dispose();
                }
                disposed = true;
            }
        }
    }
}