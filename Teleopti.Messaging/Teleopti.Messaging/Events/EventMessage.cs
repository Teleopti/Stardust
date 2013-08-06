using System;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Events
{
    public class EventMessage : IEventMessage
    {
        private Type _domainObjectTypeCache;
        private Type _referenceObjectTypeCache;

        public EventMessage()
        {
        }

        public EventMessage(Guid eventId,
                            DateTime eventStartDate,
                            DateTime eventEndDate,
                            Guid moduleId,
                            Guid referenceObjectId,
                            string referenceObjectType,
                            Guid domainObjectId,
                            string domainObjectType,
                            DomainUpdateType domainUpdateType,
                            string changedBy,
                            DateTime changedDateTime)
        {
            EventId = eventId;
            EventStartDate = eventStartDate;
            EventEndDate = eventEndDate;
            ModuleId = moduleId;
            ReferenceObjectId = referenceObjectId;
            ReferenceObjectType = referenceObjectType;
            DomainObjectId = domainObjectId;
            DomainObjectType = domainObjectType;
            DomainUpdateType = domainUpdateType;
            ChangedBy = changedBy;
            ChangedDateTime = changedDateTime;
            DomainObject = new byte[0];
        }

        public Guid EventId { get; set; }

        public DateTime EventStartDate { get; set; }

        public DateTime EventEndDate { get; set; }

        public Guid ModuleId { get; set; }

        public Guid ReferenceObjectId { get; set; }

        public string ReferenceObjectType { get; set; }

        public Type ReferenceObjectTypeCache
        {
            get
            {
                if (_referenceObjectTypeCache == null && !string.IsNullOrEmpty(ReferenceObjectType))
                    _referenceObjectTypeCache = Type.GetType(ReferenceObjectType);
                return _referenceObjectTypeCache;
            }
        }

        public Guid DomainObjectId { get; set; }

        public string DomainObjectType { get; set; }

        public Type DomainObjectTypeCache
        {
            get { return _domainObjectTypeCache ?? (_domainObjectTypeCache = Type.GetType(DomainObjectType)); }
        }

        public Type InterfaceType { get; set; }

        public DomainUpdateType DomainUpdateType { get; set; }

        public byte[] DomainObject { get; set; }

        public string ChangedBy { get; set; }

        public DateTime ChangedDateTime { get; set; }

        public int CompareTo(object obj)
        {
            IEventMessage eventMessage = obj as IEventMessage;
            if(eventMessage != null)
            {
                if (ChangedDateTime < eventMessage.ChangedDateTime)
                {
                    return -1;
                }
                if (ChangedDateTime == eventMessage.ChangedDateTime)
                {
                    return 0;
                }
                if(ChangedDateTime > eventMessage.ChangedDateTime)
                {
                    return 1;
                }
            }
            return 0;
        }
    }
}