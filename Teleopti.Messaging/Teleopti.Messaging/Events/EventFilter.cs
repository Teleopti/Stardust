using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Events
{

    /// <summary>
    /// The event filter class, used by the client upon start up, 
    /// register the events the client wants notifications of updates on.
    /// A filtering will take place on DomainObjectId and DomainObjectType.
    /// </summary>
    [Serializable]
    public class EventFilter : IEventFilter
    {

        #region Private Member variables.

        /// <summary>
        /// The filter id is just a counter
        /// of the number of filters persisted.
        /// </summary>
        private Guid _filterId;
        /// <summary>
        /// The subscription id, each user
        /// client instance will have a unique
        /// subscriber id.
        /// </summary>
        private Guid _subscriberId;
        /// <summary>
        /// The domain objecy id, as given to the domain object by hibernate,
        /// used for filtering out messages. Event Messages should only be 
        /// propagated to people interested in the domain object id and 
        /// the domain object type just updated.
        /// </summary>
        private Guid _domainObjectId;
        /// <summary>
        /// The type of the domain object, namespace and
        /// class name, used for filtering.
        /// </summary>
        private string _domainObjectType;
        /// <summary>
        /// Event Start Date, first day of range for which event is valid.
        /// </summary>
        private DateTime _eventStartDate;
        /// <summary>
        /// Event End Date, last day of range for which event is valid.
        /// </summary>
        private DateTime _eventEndDate;
        /// <summary>
        /// The user or class that created/changed the event,
        /// the space in the database is currently limited to 10 characters.
        /// </summary>
        private string _changedBy;
        /// <summary>
        /// The Date Time for which the event was created/changed.
        /// </summary>
        private DateTime _changedDateTime;
        /// <summary>
        /// The parent object id.
        /// </summary>
        private Guid _referenceObjectId;
        /// <summary>
        /// The parent objects type.
        /// </summary>
        private string _referenceObjectType;

        private Type _domainObjectTypeCache;
        private Type _referenceObjectTypeCache;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public EventFilter()
        {
        }

        /// <summary>
        /// A specific construction of an Event Filter,
        /// assigning the member variables upon construction.
        /// </summary>
        /// <param name="filterId">The filter id.</param>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <param name="referenceObjectId">The reference object id.</param>
        /// <param name="referenceObjectType">Type of the reference object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="eventStartDate">The event start date.</param>
        /// <param name="eventEndDate">The event end date.</param>
        /// <param name="changedBy">The changed by.</param>
        /// <param name="changedDateTime">The changed date time.</param>
        public EventFilter(Guid filterId,
                           Guid subscriberId,
                           Guid referenceObjectId,
                           string referenceObjectType, 
                           Guid domainObjectId, 
                           string domainObjectType, 
                           DateTime eventStartDate,
                           DateTime eventEndDate,
                           string changedBy, 
                           DateTime changedDateTime)
        {
            _filterId = filterId;
            _subscriberId = subscriberId;
            _referenceObjectId = referenceObjectId;
            _referenceObjectType = referenceObjectType;
            _domainObjectId = domainObjectId;
            _domainObjectType = domainObjectType;
            _eventStartDate = eventStartDate;
            _eventEndDate = eventEndDate;
            _changedBy = changedBy;
            _changedDateTime = changedDateTime;
        }

        /// <summary>
        /// Proteced construction used when Event Log Entry is deserialised.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected EventFilter(SerializationInfo info, StreamingContext context)
        {
            _filterId = (Guid)info.GetValue("FilterId", typeof(Guid));
            _subscriberId = (Guid)info.GetValue("SubscriberID", typeof(Guid));
            _referenceObjectId = (Guid)info.GetValue("ReferenceObjectId", typeof(Guid));
            _referenceObjectType = info.GetString("ReferenceObjectType");
            _domainObjectId = (Guid)info.GetValue("DomainObjectId", typeof(Guid));
            _domainObjectType = info.GetString("DomainObjectType");
            _eventStartDate = info.GetDateTime("EventStartDate");
            _eventEndDate = info.GetDateTime("EventEndDate");
            _changedBy = info.GetString("ChangedBy");
            _changedDateTime = info.GetDateTime("ChangedDateTime");
        }

        #endregion

        /// <summary>
        /// The filter Id for a certain filter,
        /// this is a counter of filters persisted.
        /// </summary>
        public Guid FilterId
        {
            get { return _filterId; }
            set { _filterId = value; }
        }

        /// <summary>
        /// The subscriber id, each subscription has 
        /// a unique id. one user can have several subscriptions.
        /// </summary>
        public Guid SubscriberId
        {
            get { return _subscriberId; }
            set { _subscriberId = value; }
        }

        /// <summary>
        /// The 'Domain Object ID' for the Parent Object. The hibernate key for the parent domain object.
        /// The Parent is an object that rules the existance of a child. Forinstance IPersonalAssignment
        /// belongs to one person. This enables a client to subscribe to events just concerning the current user.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/04/2009
        /// </remarks>
        public Guid ReferenceObjectId
        {
            get { return _referenceObjectId; }
            set { _referenceObjectId = value; }
        }

        /// <summary>
        /// The 'Type' of the parent in String format.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/04/2009
        /// </remarks>
        public string ReferenceObjectType
        {
            get { return _referenceObjectType; }
            set { _referenceObjectType = value; }
        }

        public Type ReferenceObjectTypeCache
        {
            get
            {
                if (_referenceObjectTypeCache == null)
                    _referenceObjectTypeCache = Type.GetType(_domainObjectType);
                return _referenceObjectTypeCache;
            }
        }

        /// <summary>
        /// Domain Object Id is used for filtering. 
        /// This filter will only propagate messages
        /// where the domain object id and the domain
        /// object type of the Event Message corresponds
        /// to this id and this domain object type below.
        /// </summary>
        public Guid DomainObjectId
        {
            get { return _domainObjectId; }
            set { _domainObjectId = value; }
        }

        /// <summary>
        /// The Domain Object Type, or Event Topic,
        /// this is the namespace and class updated.
        /// </summary>
        public string DomainObjectType
        {
            get { return _domainObjectType; }
            set { _domainObjectType = value; }
        }

        public Type DomainObjectTypeCache
        {
            get
            {
                if (_domainObjectTypeCache==null)
                    _domainObjectTypeCache = Type.GetType(_domainObjectType);
                return _domainObjectTypeCache;
            }
        }

        /// <summary>
        /// Event Start Date,
        /// the first date this event is valid,
        /// for subscription.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/04/2009
        /// </remarks>
        public DateTime EventStartDate
        {
            get { return _eventStartDate; }
            set { _eventStartDate = value; }
        }

        /// <summary>
        /// Event End Date,
        /// the last date this event is valid,
        /// for subscription.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/04/2009
        /// </remarks>
        public DateTime EventEndDate
        {
            get { return _eventEndDate; }
            set { _eventEndDate = value; }
        }

        /// <summary>
        /// The user or program that created/changed the event.
        /// </summary>
        public string ChangedBy
        {
            get { return _changedBy; }
            set { _changedBy = value; }
        }

        /// <summary>
        /// The Date Time for which the event was created/changed.
        /// </summary>
        public DateTime ChangedDateTime
        {
            get { return _changedDateTime; }
            set { _changedDateTime = value; }
        }

        /// <summary>
        /// The GetObjectData is a method needed in order to implement
        /// the ISerializable interface. This will give us the SerializationInfo 
        /// object containing a name value pair list of the properties of the object.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("FilterId", _filterId, _filterId.GetType());
            info.AddValue("SubscriberID", _subscriberId, _subscriberId.GetType());
            info.AddValue("ReferenceObjectId", _referenceObjectId, _referenceObjectId.GetType());
            info.AddValue("ReferenceObjectType", _referenceObjectType, _referenceObjectType.GetType());
            info.AddValue("DomainObjectId", _domainObjectId, _domainObjectId.GetType());
            info.AddValue("DomainObjectType", _domainObjectType, _domainObjectType.GetType());
            info.AddValue("EventStartDate", _eventStartDate, _eventStartDate.GetType());
            info.AddValue("EventEndDate", _eventEndDate, _eventEndDate.GetType());
            info.AddValue("ChangedBy", _changedBy, _changedBy.GetType());
            info.AddValue("ChangedDateTime", _changedDateTime, _changedDateTime.GetType());     
        }

    }
}
