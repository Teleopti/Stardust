using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Events
{

    /// <summary>
    /// Teleopti's Enterprise Event Message Class.
    /// This class contains the event id being raised
    /// and by whom it was raised and when.
    /// In addition it can also contain information, 
    /// a payload in form of a domain object, 
    /// serialised to a byte array. 
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes"), Serializable]
    public class EventMessage : IEventMessage
    {

        #region Private member variables

        /// <summary>
        /// EventID, Number of events (always >= 1)
        /// </summary>
        private Guid _eventId;
        /// <summary>
        /// The start date for which this event is valid
        /// </summary>
        private DateTime _eventStartDate;
        /// <summary>
        /// The end date for which this event is valid
        /// </summary>
        private DateTime _eventEndDate;
        /// <summary>
        /// The User who raised the event
        /// </summary>
        private Int32 _userId; 
        /// <summary>
        /// Domain object id, usually the unique identifier
        /// </summary>
        private Guid _domainObjectId;
        /// <summary>
        /// Domain object type, as a string
        /// </summary>
        private string _domainObjectType;      
        /// <summary>
        /// The Serialized domain object
        /// </summary>
        private byte[] _domainObject;          
        /// <summary>
        /// The update code
        /// </summary>
        private string _changedBy;
        /// <summary>
        /// The update date time
        /// </summary>
        private DateTime _changedDateTime;
        /// <summary>
        /// The id of the process that raised this message
        /// </summary>
        private int _processId;
        /// <summary>
        /// The size of this package
        /// </summary>
        private Guid _moduleId;
        /// <summary>
        /// The size of this package
        /// </summary>
        private int _packageSize;
        /// <summary>
        /// The heart beat
        /// </summary>
        private bool _isHeartBeat;
        /// <summary>
        /// Insert, Update or delete
        /// </summary>
        private DomainUpdateType _domainUpdateType;
        [NonSerialized]
        private bool _isInterprocess;
        /// <summary>
        /// Interface Type in System.Type format
        /// </summary>
        private Type _interfaceType;
        /// <summary>
        /// The parent object id.
        /// </summary>
        private Guid _referenceObjectId;
        /// <summary>
        /// The parent object type.
        /// </summary>
        private string _referenceObjectType;

        private Type _domainObjectTypeCache;
        private Type _referenceObjectTypeCache;

        #endregion

        #region Constructors

        /// <summary>
        /// The Event Message's default constructor.
        /// </summary>
        public EventMessage()
        {
        }

        /// <summary>
        /// The 'FULL' constructor, which also takes the serialised object.
        /// </summary>
        /// <param name="eventId">The event id.</param>
        /// <param name="eventStartDate">The event start date.</param>
        /// <param name="eventEndDate">The event end date.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="processId">The process id.</param>
        /// <param name="moduleId">The module id.</param>
        /// <param name="packageSize">Size of the package.</param>
        /// <param name="isHeartbeat">if set to <c>true</c> [is heartbeat].</param>
        /// <param name="referenceObjectId">The parent object id.</param>
        /// <param name="referenceObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="domainUpdateType">Type of the domain update.</param>
        /// <param name="domainObject">The domain object.</param>
        /// <param name="changedBy">The changed by.</param>
        /// <param name="changedDateTime">The changed date time.</param>
        public EventMessage(Guid eventId,
                            DateTime eventStartDate,
                            DateTime eventEndDate,
                            Int32 userId,
                            Int32 processId,
                            Guid moduleId,
                            Int32 packageSize,
                            bool isHeartbeat,
                            Guid referenceObjectId,
                            string referenceObjectType,
                            Guid domainObjectId,
                            string domainObjectType,
                            DomainUpdateType domainUpdateType,
                            byte[] domainObject,
                            string changedBy,
                            DateTime changedDateTime)
        {
            _eventId = eventId;
            _eventStartDate = eventStartDate;
            _eventEndDate = eventEndDate;
            _userId = userId;
            _processId = processId;
            _moduleId = moduleId;
            _packageSize = packageSize;
            _isHeartBeat = isHeartbeat;
            _referenceObjectId = referenceObjectId;
            _referenceObjectType = referenceObjectType;
            _domainObjectId = domainObjectId;
            _domainObjectType = domainObjectType;
            _domainUpdateType = domainUpdateType;
            _domainObject = domainObject;
            _changedBy = changedBy; 
            _changedDateTime = changedDateTime;
        }

        /// <summary>
        /// Constructor that does not take the serialised object.
        /// </summary>
        /// <param name="eventId">The event id.</param>
        /// <param name="eventStartDate">The event start date.</param>
        /// <param name="eventEndDate">The event end date.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="processId">The process id.</param>
        /// <param name="moduleId">The module id.</param>
        /// <param name="packageSize">Size of the package.</param>
        /// <param name="isHeartbeat">if set to <c>true</c> [is heartbeat].</param>
        /// <param name="referenceObjectId">The parent object id.</param>
        /// <param name="referenceObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="domainUpdateType">Type of the domain update.</param>
        /// <param name="changedBy">The changed by.</param>
        /// <param name="changedDateTime">The changed date time.</param>
        public EventMessage(Guid eventId,
                            DateTime eventStartDate,
                            DateTime eventEndDate,
                            Int32 userId,
                            Int32 processId,
                            Guid moduleId,
                            Int32 packageSize,
                            bool isHeartbeat,
                            Guid referenceObjectId,
                            string referenceObjectType,
                            Guid domainObjectId,
                            string domainObjectType,
                            DomainUpdateType domainUpdateType,
                            string changedBy,
                            DateTime changedDateTime)
        {
            _eventId = eventId;
            _eventStartDate = eventStartDate;
            _eventEndDate = eventEndDate;
            _userId = userId;
            _processId = processId;
            _moduleId = moduleId;
            _packageSize = packageSize;
            _isHeartBeat = isHeartbeat;
            _referenceObjectId = referenceObjectId;
            _referenceObjectType = referenceObjectType;
            _domainObjectId = domainObjectId;
            _domainObjectType = domainObjectType;
            _domainUpdateType = domainUpdateType;
            _changedBy = changedBy;
            _changedDateTime = changedDateTime;
            _domainObject = new byte[0];
        }

        /// <summary>
        /// Construction EventMessage upon serialisation information.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected EventMessage(SerializationInfo info, StreamingContext context)
        {
            _eventId = (Guid)info.GetValue("EventId", typeof(Guid));
            _eventStartDate = info.GetDateTime("StartDate");
            _eventEndDate = info.GetDateTime("EndDate");
            _userId = info.GetInt32("UserId");
            _processId = info.GetInt32("ProcessId");
            _moduleId = new Guid(info.GetString("ModuleId"));
            _packageSize = info.GetInt32("PackageSize");
            _isHeartBeat = info.GetBoolean("IsHeartBeat");
            _referenceObjectId = (Guid)info.GetValue("ReferenceObjectId", typeof(Guid));
            _referenceObjectType = info.GetString("ReferenceObjectType");
            _domainObjectId = (Guid)info.GetValue("DomainObjectId", typeof(Guid));
            _domainObjectType = info.GetString("DomainObjectType");
            _domainUpdateType = (DomainUpdateType) info.GetValue("DomainUpdateType", typeof (DomainUpdateType));
            _domainObject = (byte[]) info.GetValue("DomainObject", typeof(byte[]));
            _changedBy = info.GetString("ChangedBy");
            _changedDateTime = info.GetDateTime("ChangedDateTime");
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Each type of event has an ID.
        /// </summary>
        public Guid EventId
        {
            get { return _eventId; }
            set { _eventId = value; }
        }

        /// <summary>
        /// Start Date for which this partiuclar event is valid.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        public DateTime EventStartDate
        {
            get { return _eventStartDate; }
            set { _eventStartDate = value; }
        }

        /// <summary>
        /// End Date for which this partiuclar event is valid.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        public DateTime EventEndDate
        {
            get { return _eventEndDate; }
            set { _eventEndDate = value; }
        }

        /// <summary>
        /// The User ID.
        /// Who raised the event?  
        /// </summary>
        public Int32 UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        /// <summary>
        /// The id of the process from which this message was sent.
        /// </summary>
        public int ProcessId
        {
            get { return _processId; }
            set { _processId = value; }
        }

        /// <summary>
        /// The module id within the process
        /// </summary>
        public Guid ModuleId
        {
            get { return _moduleId; }
            set { _moduleId = value; }
        }

        /// <summary>
        /// The size of this package
        /// </summary>
        public int PackageSize
        {
            get { return _packageSize; }
            set { _packageSize = value; }
        }

        /// <summary>
        /// States whether this is a heart beat or not.
        /// </summary>
        public bool IsHeartbeat
        {
            get { return _isHeartBeat; }
            set { _isHeartBeat = value; }
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
                if (_referenceObjectTypeCache == null && !string.IsNullOrEmpty(_referenceObjectType))
                    _referenceObjectTypeCache = Type.GetType(_referenceObjectType);
                return _referenceObjectTypeCache;
            }
        }

        /// <summary>
        /// The 'Domain Object ID'. The hibernate key for
        /// the domain object that is payload for the event.
        /// This key needs to be populated event if the 
        /// acctual Domain Object is not sent as payload
        ///  in serialised form.
        /// </summary>
        public Guid DomainObjectId
        {
            get { return _domainObjectId; }
            set { _domainObjectId = value; }
        }

        /// <summary>
        /// The 'Type' of the domain object as the name implies.
        /// </summary>
        public String DomainObjectType
        {
            get { return _domainObjectType; }
            set { _domainObjectType = value; }
        }

        public Type DomainObjectTypeCache
        {
            get
            {
                if (_domainObjectTypeCache == null)
                    _domainObjectTypeCache = Type.GetType(_domainObjectType);
                return _domainObjectTypeCache;
            }
        }

        /// <summary>
        /// The 'Type' in System.Type format,
        /// it needs to be an interface
        /// in order for the ASM to work.
        /// This Property is set by the MessageBroker.
        /// </summary>
        /// <value></value>
        public Type InterfaceType
        {
            get { return _interfaceType; }
            set { _interfaceType = value; }
        }

        /// <summary>
        /// Insert, Update or Delete?
        /// </summary>
        /// <value></value>
        public DomainUpdateType DomainUpdateType
        {
            get { return _domainUpdateType; }
            set { _domainUpdateType = value; }
        }

        /// <summary>
        /// The serialised Domain Object
        /// in form of a byte array. 
        /// </summary>
        public byte[] DomainObject
        {
            get { return _domainObject; }
            set { _domainObject = value; }
        }

        /// <summary>
        /// The update program code. This program update
        /// code represents the namespace and class name of the 
        /// code that created / updated the 'Event Message'
        /// or the User and Domain of person who raised the Event.
        /// </summary>
        public string ChangedBy
        {
            get { return _changedBy; }
            set { _changedBy = value; }
        }

        /// <summary>
        /// When Event Message was created / updated.
        /// </summary>
        public DateTime ChangedDateTime
        {
            get { return _changedDateTime; }
            set { _changedDateTime = value; }
        }

        /// <summary>
        /// Is this event message generated from the same process
        /// </summary>
        /// <value></value>
        public bool IsInterprocess
        {
            get { return _isInterprocess; }
            set { _isInterprocess = value; }
        }

        #endregion

        /// <summary>
        /// Override of the ToString method
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            String value = "Event ID = " + _eventId + Environment.NewLine +
                           "Event Start Date = " + _eventStartDate + Environment.NewLine +
                           "Event End Date = " + _eventEndDate + Environment.NewLine +
                           "User ID = " + _userId + Environment.NewLine +
                           "Reference Object ID = " + _referenceObjectId + Environment.NewLine +
                           "Reference Object Type = " + _referenceObjectType + Environment.NewLine +
                           "Domain Object ID = " + _domainObjectId + Environment.NewLine +
                           "Domain Object Type = " + _domainObjectType + Environment.NewLine +
                           "Domain Update Type = " + _domainUpdateType + Environment.NewLine +
                           "Changed By = " + _changedBy + Environment.NewLine +
                           "Changed Date Time = " + _changedDateTime;
            return value;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings:
        /// Value
        /// Meaning
        /// Less than zero
        /// This instance is less than <paramref name="obj"/>.
        /// Zero
        /// This instance is equal to <paramref name="obj"/>.
        /// Greater than zero
        /// This instance is greater than <paramref name="obj"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        /// 	<paramref name="obj"/> is not the same type as this instance.
        /// </exception>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        public int CompareTo(object obj)
        {
            IEventMessage eventMessage = obj as IEventMessage;
            if(eventMessage != null)
            {
                if (_changedDateTime < eventMessage.ChangedDateTime)
                {
                    return -1;
                }
                if (_changedDateTime == eventMessage.ChangedDateTime)
                {
                    return 0;
                }
                if(_changedDateTime > eventMessage.ChangedDateTime)
                {
                    return 1;
                }
            }
            return 0;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("EventId", _eventId, _eventId.GetType());
            info.AddValue("StartDate", _eventStartDate, _eventStartDate.GetType());
            info.AddValue("EndDate", _eventEndDate, _eventEndDate.GetType());
            info.AddValue("UserId", _userId, _userId.GetType());
            info.AddValue("ProcessId", _processId, _processId.GetType());
            info.AddValue("ModuleId", _moduleId, _moduleId.GetType());
            info.AddValue("PackageSize", _packageSize, _packageSize.GetType());
            info.AddValue("IsHeartBeat", _isHeartBeat, _userId.GetType());
            info.AddValue("ReferenceObjectId", _referenceObjectId, _referenceObjectId.GetType());
            info.AddValue("ReferenceObjectType", _referenceObjectType, _referenceObjectType.GetType());
            info.AddValue("DomainObjectId", _domainObjectId, _domainObjectId.GetType());
            info.AddValue("DomainObjectType", _domainObjectType, _domainObjectType.GetType());
            info.AddValue("DomainUpdateType", _domainUpdateType, _domainUpdateType.GetType());
            info.AddValue("DomainObject", _domainObject, _domainObject.GetType());
            info.AddValue("ChangedBy", _changedBy, _changedBy.GetType());
            info.AddValue("ChangedDateTime", _changedDateTime, _changedDateTime.GetType());           
        }


    }
}