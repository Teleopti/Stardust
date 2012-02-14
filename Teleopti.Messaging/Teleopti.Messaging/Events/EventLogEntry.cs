using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Schema;
using Teleopti.Messaging.Interfaces.Events;
using System.Globalization;

namespace Teleopti.Messaging.Events
{

    /// <summary>
    /// Teleopti's event log entry class is
    /// used for keeping track of the event 
    /// generation and distribution.
    /// The log entries can be monitored using
    /// Teleopti's Messaging Management Tool.
    /// </summary>
    [Serializable]
    public class EventLogEntry : IEventLogEntry
    {
        #region Private Member Variables

        private Guid _logId;
        private Int32 _processId;
        private string _description;
        private string _exception;
        private string _message;
        private string _stackTrace;
        private string _changedBy;
        private DateTime _changedDateTime;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for an Event Log Entry.
        /// </summary>
        public EventLogEntry()
        {

        }

        /// <summary>
        /// Specific instructor for an Event Log Entry.
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="processId"></param>
        /// <param name="description"></param>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        /// <param name="changedBy"></param>
        /// <param name="changedDateTime"></param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "endPoint")]
        public EventLogEntry(Guid logId,
                             Int32 processId,
                             string description,
                             string exception,
                             string message,
                             string stackTrace,
                             string changedBy,
                             DateTime changedDateTime)
        {
            _logId = logId;
            _processId = processId;
            _description = description;
            _exception = exception;
            _message = message;
            _stackTrace = stackTrace;
            _changedBy = changedBy;
            _changedDateTime = changedDateTime;
        }
        
        /// <summary>
        /// Proteced construction used when Event Log Entry is deserialised.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected EventLogEntry(SerializationInfo info, StreamingContext context)
        {
            _logId = (Guid) info.GetValue("LogId", typeof(Guid));
            _processId = info.GetInt32("ProcessId");
            _description = info.GetString("Description");
            _exception = info.GetString("Exception");
            _message = info.GetString("Message");
            _stackTrace = info.GetString("StackTrace");
            _changedBy = info.GetString("ChangedBy");
            _changedDateTime = info.GetDateTime("ChangedDateTime");
        }

        #endregion

        /// <summary>
        /// The Id of the log entry.
        /// </summary>
        public Guid LogId
        {
            get { return _logId; }
            set { _logId = value; }
        }

        /// <summary>
        /// The IP Address, the end point, of the subscriber.
        /// </summary>
        public Int32 ProcessId
        {
            get { return _processId; }
            set { _processId = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// The short form of an exception. 50 Characters long.
        /// </summary>
        public string Exception
        {
            get { return _exception; }
            set { _exception = value; }
        }

        /// <summary>
        /// The short form of the message. 50 Characters long.
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        /// <summary>
        /// The stack traces of the exception logged. 50 Characters long.
        /// </summary>
        public string StackTrace
        {
            get { return _stackTrace; }
            set { _stackTrace = value; }
        }

        /// <summary>
        /// The log entry was generated/created/changed by a user or
        /// a program namespace, restricted to 10 characters so it needs
        /// to be abbreviated.
        /// </summary>
        public string ChangedBy
        {
            get { return _changedBy; }
            set { _changedBy = value; }
        }

        /// <summary>
        /// When the log entry was generated/created/changed.
        /// </summary>
        public DateTime ChangedDateTime
        {
            get { return _changedDateTime; }
            set { _changedDateTime = value; }
        }

        /// <summary>
        /// The GetObjectData is used to get the SerializationInfo object containing 
        /// a name value pair list of the Event Log Entry's properties and their values.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("LogId", _logId, _logId.GetType());
            info.AddValue("ProcessId", _processId, _processId.GetType());
            info.AddValue("Description", _description, _description.GetType());
            info.AddValue("Exception", _exception, _exception.GetType());
            info.AddValue("Message", _message, _message.GetType());
            info.AddValue("StackTrace", _stackTrace, _stackTrace.GetType());
            info.AddValue("ChangedBy", _changedBy, _changedBy.GetType());
            info.AddValue("ChangedDateTime", _changedDateTime, _changedDateTime.GetType());     
        }



    }
}
