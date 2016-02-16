using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.MessageBroker.Legacy
{
    public class EventMessage : IEventMessage
    {
        private Type _domainObjectTypeCache;

        public EventMessage()
        {
        }

        public EventMessage(Guid eventId,
                            DateTime eventStartDate,
                            DateTime eventEndDate,
                            Guid moduleId,
                            Guid referenceObjectId,
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
        public Guid DomainObjectId { get; set; }
        public string DomainObjectType { get; set; }

        public Type InterfaceType { get; set; }
        public DomainUpdateType DomainUpdateType { get; set; }
        public byte[] DomainObject { get; set; }
        public string ChangedBy { get; set; }
        public DateTime ChangedDateTime { get; set; }

		public Type DomainObjectTypeCache
		{
			get { return _domainObjectTypeCache ?? (_domainObjectTypeCache = Type.GetType(DomainObjectType)); }
		}

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