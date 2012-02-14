using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Logging.Core
{
    [Serializable, SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
    public class LogbookEntry : ILogbookEntry
    {
        private readonly ILogEntry _logEntry;
        private readonly IEventMessage _eventMessage;
        private string _logType;
        private DateTime _logDateTime;
        private string _logMessage;
        private string _classType;
        private string _changedBy;

        public LogbookEntry(ILogEntry logEntry)
        {

            _logEntry = logEntry;
            
            Intialise();

        }

        public LogbookEntry(IEventMessage eventMessage)
        {
            _eventMessage = eventMessage;

            IntialiseFromEventMessage();

        }

        /// <summary>
        /// Proteced construction used when Log Book Entry is deserialised.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected LogbookEntry(SerializationInfo info, StreamingContext context)
        {
            _logType = info.GetString("LogType");
            _logDateTime = info.GetDateTime("LogDateTime");
            _logMessage = info.GetString("LogMessage");
            _classType = info.GetString("ClassType");
            _changedBy = info.GetString("ChangedBy");
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void IntialiseFromEventMessage()
        {
            string[] values = Encoding.GetEncoding(Consts.DefaultCharEncoding).GetString(_eventMessage.DomainObject).Split(new char[] { ',' });
            try
            {
                if (values.Length == 4)
                {
                    _logDateTime = _eventMessage.ChangedDateTime;
                    _logType = values[1];
                    _classType = values[2];
                    _logMessage = values[3].TrimEnd(new char[]{'\0'});
                    _changedBy = _eventMessage.ChangedBy;
                }
                else
                {
                    _logDateTime = _eventMessage.ChangedDateTime;
                    _logType = values[1];
                    _classType = "NotApplicable";
                    _logMessage = values[2].TrimEnd(new char[] { '\0' });
                    _changedBy = _eventMessage.ChangedBy;
                }
            }
            catch (Exception)
            {
                _logDateTime = DateTime.Now;
                _logType = "Unknown";
                _classType = "Unknown";
                _logMessage = _eventMessage.DomainObjectType;
                _changedBy = _eventMessage.ChangedBy;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Intialise()
        {
            string[] values = _logEntry.Description.Split(new char[] {','});
            try
            {
                if(values.Length == 4)
                {
                    _logDateTime = DateTime.Parse(values[0], CultureInfo.InvariantCulture);
                    _logType = values[1];
                    _classType = values[2];
                    _logMessage = values[3];
                    _changedBy = _logEntry.ChangedBy;
                }
                else
                {
                    _logDateTime = DateTime.Parse(values[0], CultureInfo.InvariantCulture);
                    _logType = values[1];
                    _classType = "NotApplicable";
                    _logMessage = values[2];
                    _changedBy = _logEntry.ChangedBy;
                }
            }
            catch(Exception)
            {
                _logDateTime = DateTime.Now;
                _logType = "UNKNOWN";
                _classType = "UNKNOWN";
                _logMessage = _logEntry.Description;
                _changedBy = _logEntry.ChangedBy;
            }
        }

        public DateTime LogDateTime
        {
            get { return _logDateTime; }
        }

        public string LogType
        {
            get { return _logType; }
        }

        public string ClassType
        {
            get { return _classType; }
        }

        public string LogMessage
        {
            get { return _logMessage; }
        }

        public string UserName
        {
            get { return _changedBy; }
        }

        public ILogEntry LogEntry
        {
            get { return _logEntry; }
        }

        public IEventMessage EventMessage
        {
            get { return _eventMessage; }
        }

        /// <summary>
        /// The GetObjectData is used to get the SerializationInfo object containing 
        /// a name value pair list of the Log Book Entry's properties and their values.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("LogType", _logType, _logType.GetType());
            info.AddValue("LogDateTime", _logDateTime, _logDateTime.GetType());
            info.AddValue("LogMessage", _logMessage, _logMessage.GetType());
            info.AddValue("ClassType", _classType, _classType.GetType());
            info.AddValue("ChangedBy", _changedBy, _changedBy.GetType());
        }

        public int CompareTo(object obj)
        {
            ILogbookEntry entry = (ILogbookEntry) obj;
            if(entry.LogDateTime > LogDateTime)
            {
                return 1;
            }
            else if(entry.LogDateTime < LogDateTime)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

    }
}