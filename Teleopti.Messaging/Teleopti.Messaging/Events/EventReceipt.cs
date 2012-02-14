using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Events
{
    [Serializable]
    public class EventReceipt : IEventReceipt
    {
        private Guid _receiptId;
        private Guid _eventId;
        private Int32 _processId;
        private string _changedBy;
        private DateTime _changedDateTime;

        public EventReceipt()
        {
        }

        public EventReceipt(Guid receiptId, Guid eventId, Int32 processId, string changedBy, DateTime changedDateTime)
        {
            _receiptId = receiptId;
            _eventId = eventId;
            _processId = processId;
            _changedBy = changedBy;
            _changedDateTime = changedDateTime;
        }

        protected EventReceipt(SerializationInfo info, StreamingContext context)
        {
            _receiptId = (Guid)info.GetValue("ReceiptId", typeof(Guid));
            _eventId = (Guid)info.GetValue("EventId", typeof(Guid));
            _processId = info.GetInt32("ProcessId");
            _changedBy = info.GetString("ChangedBy");
            _changedDateTime = info.GetDateTime("ChangedDateTime");
        }

        public Guid ReceiptId
        {
            get { return _receiptId; }
            set { _receiptId = value; }
        }

        public Guid EventId
        {
            get { return  _eventId; }
            set { _eventId = value; }
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
            info.AddValue("ReceiptId", _receiptId, _receiptId.GetType());
            info.AddValue("EventId", _eventId, _eventId.GetType());
            info.AddValue("ProcessId", _processId, _processId.GetType());
            info.AddValue("ChangedBy", _changedBy, _changedBy.GetType());
            info.AddValue("ChangedDateTime", _changedDateTime, _changedDateTime.GetType());            
        }

    }
}
