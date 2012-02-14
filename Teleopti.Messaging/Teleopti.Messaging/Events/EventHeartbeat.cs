using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Events
{
    [Serializable]
    public class EventHeartbeat : IEventHeartbeat
    {
        private Guid _heartbeatId;
        private Guid _subscriberId;
        private Int32 _processId;
        private string _changedBy;
        private DateTime _changedDateTime;

        public EventHeartbeat()
        {
        }

        public EventHeartbeat(Guid heartbeatId, Guid subscriberId, Int32 processId, string changedBy, DateTime changedDateTime)
        {
            _heartbeatId = heartbeatId;
            _subscriberId = subscriberId;
            _processId = processId;
            _changedBy = changedBy;
            _changedDateTime = changedDateTime;
        }

        protected EventHeartbeat(SerializationInfo info, StreamingContext context)
        {
            _heartbeatId = (Guid)info.GetValue("HeartbeatId", typeof(Guid));
            _subscriberId = (Guid)info.GetValue("SubscriberId", typeof(Guid));
            _processId = info.GetInt32("ProcessId");
            _changedBy = info.GetString("ChangedBy");
            _changedDateTime = info.GetDateTime("ChangedDateTime");
        }

        public Guid HeartbeatId
        {
            get { return _heartbeatId; }
            set { _heartbeatId = value; }
        }

        public Guid SubscriberId
        {
            get { return _subscriberId; }
            set { _subscriberId = value; }
        }

        public int ProcessId
        {
            get { return _processId; }
            set { _processId = value; }
        }

        public string ChangedBy
        {
            get { return _changedBy; }
            set { _changedBy = value; }
        }

        public DateTime ChangedDateTime
        {
            get { return _changedDateTime; }
            set { _changedDateTime = value; }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("HeartbeatId", _heartbeatId, _heartbeatId.GetType());
            info.AddValue("SubscriberId", _subscriberId, _subscriberId.GetType());
            info.AddValue("ProcessId", _processId, _processId.GetType());
            info.AddValue("ChangedBy", _changedBy, _changedBy.GetType());
            info.AddValue("ChangedDateTime", _changedDateTime, _changedDateTime.GetType()); 
        }

    }
}
