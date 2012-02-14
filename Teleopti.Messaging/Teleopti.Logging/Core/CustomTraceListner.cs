using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;
using Teleopti.Core;

namespace Teleopti.Logging.Core
{
    public class CustomTraceListener : TraceListener
    {
        private string _fileName;
        private double _rollOverIntervall;
        private DateTime _currentDate;
        private StreamWriter _traceWriter;
        private bool _disposed;

        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "lock")]
        private static readonly object _lockObject = new Object();
        private bool _isRollOverChecked;

        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        private readonly CustomThreadPool _queueReader = new CustomThreadPool(1, "Teleopti Logging Thread");

        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "application")]
        protected const string ApplicationName = "RaptorMessaging";

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "log")]
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        protected const string LogName = "EventMessageLog";

        protected CustomThreadPool QueueReader
        {
            get { return _queueReader; }
        }

        protected static object LockObject
        {
            get { return _lockObject; }
        }

        // English spelling ...
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Initialise")]
        public virtual void Initialise(string fileName, bool rollover)
        {
            _rollOverIntervall = 30; // 30 minutes by default
            _fileName = fileName;
            _isRollOverChecked = rollover;
            CreateStream();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Initialise")]
        public virtual void Initialise(string fileName, double rolloverInterval)
        {
            _rollOverIntervall = rolloverInterval;
            _fileName = fileName;
            CreateStream();
        }

        private void CreateStream()
        {
            _traceWriter = new StreamWriter(_fileName, true);
        }

        public override void Write(string message)
        {
            QueueReader.QueueUserWorkItem(new WaitCallback(WriteMessage), message);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)")]
        public override void WriteLine(string message)
        {
            message = String.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Empty", message);
            QueueReader.QueueUserWorkItem(new WaitCallback(WriteLineMessage), message);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)")]
        public override void WriteLine(string message, string category)
        {
            message = String.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), category, message);
            QueueReader.QueueUserWorkItem(new WaitCallback(WriteLineMessage), message);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)")]
        public virtual void WriteEventLogEntry(string message, EventLogEntryType eventType)
        {
            message = String.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), eventType, message);
            LogItem item = new LogItem(message, eventType);
            QueueReader.QueueUserWorkItem(new WaitCallback(WriteEventLogEntry), item);
        }

        private void WriteMessage(object obj)
        {
            lock (LockObject)
            {
                CheckRollover();
                string message = (string)obj;
                if (_traceWriter != null)
                {
                    _traceWriter.Write(message);
                    _traceWriter.Flush();
                }
            }
        }

        private void WriteLineMessage(object obj)
        {
            lock (LockObject)
            {
                CheckRollover();
                string message = (string)obj;
                if (_traceWriter != null)
                {
                    _traceWriter.WriteLine(message);
                    _traceWriter.Flush();
                }
            }
        }

        private void WriteEventLogEntry(object obj)
        {
            lock (LockObject)
            {
                CheckRollover();
                LogItem item = (LogItem)obj;
                WriteLogEntry(item.Message, item.LogEntryType);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private void WriteLogEntry(string message, EventLogEntryType eventType)
        {
            EventLog eventLog = new EventLog();
            try
            {
                message = String.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", DateTime.Now.ToLongTimeString(), eventType, message);
                //Register the Application as an Event Source
                if (!EventLog.SourceExists(ApplicationName))
                {
                    EventLog.CreateEventSource(ApplicationName, LogName);
                }
                //log the entry
                eventLog.Source = ApplicationName;
                eventLog.WriteEntry(message, eventType);
            }
            catch (Exception)
            {
                /* CATCH ALL */
            }
        }

        private string GenerateFilename()
        {
            _currentDate = DateTime.Now;
            string curpath = Path.GetTempPath();
            _fileName = String.Format(CultureInfo.InvariantCulture, "{0}{1}", curpath, "log.csv");
            return Path.Combine(Path.GetDirectoryName(_fileName), Path.GetFileNameWithoutExtension(_fileName) + _currentDate.ToString("_yyyyMMdd_HHmm", CultureInfo.InvariantCulture) + Path.GetExtension(_fileName));
        }

        private void CheckRollover()
        {
            if (_currentDate.AddMinutes(_rollOverIntervall) < DateTime.Now && _isRollOverChecked)
            {
                CreateNew(GenerateFilename());
            }
        }

        private void CreateNew(object state)
        {
            string fileName = (string)state;
            _traceWriter.Close();
            _traceWriter = new StreamWriter(fileName, false);
        }

        protected override void Dispose(bool disposing)
        {
            if(!_disposed)
            {
                if (disposing)
                {
                    _queueReader.Dispose();
                    _traceWriter.Close();
                }
                else
                {
                    _traceWriter.Close();
                }
                _disposed = true;                
            }
        }

    }

    public class LogItem
    {
        private readonly string _message;
        private readonly EventLogEntryType _type;

        public LogItem(string message, EventLogEntryType eventLogEntryType)
        {
            _message = message;
            _type = eventLogEntryType;
        }

        public string Message
        {
            get { return _message; }
        }

        public EventLogEntryType LogEntryType
        {
            get { return _type; }
        }

    }
}