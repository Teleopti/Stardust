using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Events
{

    /// <summary>
    /// Teleopti's Event Subscriber class contains information on the current 
    /// user that is subscribing to events, what End Point the user can be found at,
    /// who changed/created the object and when this was done.
    /// One user in the system can have several subscriptions, 
    /// e.g when the user has several clients running.
    /// </summary>
    [Serializable]
    public class EventSubscriber : IEventSubscriber
    {

        #region Private Variables

        /// <summary>
        /// Subscriber Id is unique for a particular subscriber but not user.
        /// One user can have multiple subscriptions and thus be found on several
        /// entries in the Subscriber table.
        /// </summary>
        private Guid _subscriberId;
        /// <summary>
        /// The user id is the Id of the current user as designated by hibernate.
        /// </summary>
        private Int32 _userId;
        /// <summary>
        /// The End Point is the IP Adress of the subscriber.
        /// </summary>
        private Int32 _processId;
        /// <summary>
        /// The entity was changed/created by a certain user or a program.
        /// </summary>
        private string _changedBy;
        /// <summary>
        /// This property tells us when the Subscription was established.
        /// </summary>
        private DateTime _changedDateTime;
        /// <summary>
        /// The IP Address.
        /// </summary>
        private string _ipAddress;
        /// <summary>
        /// The Port number.
        /// </summary>
        private int _port;

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor for Subscriber
        /// </summary>
        public EventSubscriber()
        {
        }

        /// <summary>
        /// Specific constructor which initialises all the 
        /// private member variables upon construction.
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <param name="userId"></param>
        /// <param name="processId"></param>
        /// <param name="port"></param>
        /// <param name="changedBy"></param>
        /// <param name="changedDateTime"></param>
        /// <param name="ipAddress"></param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "endPoint")]
        public EventSubscriber(Guid subscriberId, Int32 userId, Int32 processId, string ipAddress, int port, string changedBy, DateTime changedDateTime)
        {
            _subscriberId = subscriberId;
            _userId = userId;
            _processId = processId;
            _ipAddress = ipAddress;
            _port = port;
            _changedBy = changedBy;
            _changedDateTime = changedDateTime;
        }

        /// <summary>
        /// Construction EventMessage serialisation info.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected EventSubscriber(SerializationInfo info, StreamingContext context)
        {
            _subscriberId = (Guid)info.GetValue("SubscriberId", typeof(Guid));
            _userId = info.GetInt32("UserId");
            _processId = info.GetInt32("ProcessId");
            _ipAddress = info.GetString("IPAddress");
            _port = info.GetInt32("Port");
            _changedBy = info.GetString("ChangedBy");
            _changedDateTime = info.GetDateTime("ChangedDateTime");
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The unique subscriber Id. One user can have multiple Subscriptions.
        /// </summary>
        public Guid SubscriberId
        {
            get { return _subscriberId; }
            set { _subscriberId = value; }
        }

        /// <summary>
        /// The unique user id.
        /// </summary>
        public Int32 UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        /// <summary>
        /// The process id of the subscriber.
        /// </summary>
        /// <value></value>
        public Int32 ProcessId
        {
            get { return _processId; }
            set { _processId = value; }
        }

        /// <summary>
        /// Gets or sets the IP address.
        /// </summary>
        /// <value>The IP address.</value>
        public string IPAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }
        }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        /// <summary>
        /// Who changed/created the Subscription?
        /// </summary>
        public string ChangedBy
        {
            get { return _changedBy; }
            set { _changedBy = value; }
        }

        /// <summary>
        /// When was the subscription created/changed?
        /// </summary>
        public DateTime ChangedDateTime
        {
            get { return _changedDateTime; }
            set { _changedDateTime = value; }
        }

        #endregion

        #region Serialization

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
            info.AddValue("SubscriberId", _subscriberId, _subscriberId.GetType());
            info.AddValue("UserId", _userId, _userId.GetType());
            info.AddValue("ProcessId", _processId, _processId.GetType());
            info.AddValue("IPAddress", _ipAddress, _ipAddress.GetType());
            info.AddValue("Port", _port, _port.GetType());
            info.AddValue("ChangedBy", _changedBy, _changedBy.GetType());
            info.AddValue("ChangedDateTime", _changedDateTime, _changedDateTime.GetType());     
        }

        #endregion

    }
}
