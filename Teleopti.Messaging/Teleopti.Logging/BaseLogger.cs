using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.Remoting;
using Teleopti.Logging.Core;
using Teleopti.Logging.Properties;

namespace Teleopti.Logging
{
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
    public class BaseLogger : IDisposable
    {
        private const string AssemblyName = "Teleopti.Logging";
        private static readonly object _lockObject = new object();
        private static BaseLogger _logger; 
        private CustomTraceListener _myWriter;
        private static bool _rollover;
        private static string _typeOfListener;
        private static string _path;
        private bool _disposed;

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods"), SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        public static BaseLogger Instance
        {
            get
            {
                lock (_lockObject)
                {
                    if(_logger == null)
                        _logger = new BaseLogger();
                }
                return _logger;
            }
        }

        private static string GenerateFilename(string fileName)
        {
            return Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + DateTime.Now.ToString("_yyyyMMdd_HHmm", CultureInfo.InvariantCulture) + Path.GetExtension(fileName));
        }

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        public static BaseLogger GetInstance(string typeOfListener, string path, bool rollover)
        {
            lock (_lockObject)
            {
                if (_logger == null)
                {
                    _typeOfListener = typeOfListener;
                    _path = path;
                    _rollover = rollover;
                    _logger = new BaseLogger();
                }
            }
            return _logger;
        }

        protected BaseLogger()
        {
            if(String.IsNullOrEmpty(_typeOfListener))
            {
                _typeOfListener = Resources.TypeOfTraceListner;
                _path = GenerateFilename(GenerateFilename(String.Format(CultureInfo.InvariantCulture, "{0}{1}", Path.GetTempPath(), "log.csv")));
                _rollover = true;
            }
            Initialise();
        }

        // ReSharper disable MemberCanBeMadeStatic
        private void Initialise()
        {
            ObjectHandle instance = Activator.CreateInstance(AssemblyName, _typeOfListener);
            if (instance != null) 
            {
            _myWriter = (CustomTraceListener) instance.Unwrap();
            _myWriter.Initialise(_path, _rollover);
        }
        }

        /// <summary>
        /// Category is Error, Warning, Info.
        /// Type of the object where Logging takes place.
        /// Message is the logging message.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public void WriteLine(EventLogEntryType category, Type type, string message)
        {
            string stringCategory = category.ToString();
            if (type != null)
            {
                stringCategory = String.Format(CultureInfo.InvariantCulture, "{0},{1}", stringCategory, type.FullName);
            }
            _myWriter.WriteLine(message, stringCategory);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!_disposed)
            {
                if (isDisposing)
                {
                    _myWriter.Dispose();
                }
                _disposed = true;
            }
        }

    }
}