using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Core
{
    [Serializable]
    public class MessageInformation : IMessageInformation
    {
        private int _addressId;
        private string _address;
        private int _port;
        private int _timeToLive;
        private byte[] _package;
        private IEventMessage _eventMessage;
        private Guid _subscriberId;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageInformation"/> class.
        /// </summary>
        public MessageInformation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageInformation"/> class.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <param name="addressId">The address id.</param>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        public MessageInformation(Guid subscriberId, int addressId, string address, int port) : this(subscriberId, addressId, address, port, 1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageInformation"/> class.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <param name="addressId">The address id.</param>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="timeToLive">The time to live.</param>
        public MessageInformation(Guid subscriberId, int addressId, string address, int port, int timeToLive)
        {
            _subscriberId = subscriberId;
            _addressId = addressId;
            _address = address;
            _port = port;
            _timeToLive = timeToLive;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageInformation"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected MessageInformation(SerializationInfo info, StreamingContext context)
        {
            _subscriberId = (Guid) info.GetValue("SubscriberId", typeof(Guid));
            _addressId = info.GetInt32("AddressId");
            _address = info.GetString("Address");
            _port = info.GetInt32("Port");
            _timeToLive = info.GetInt32("TimeToLive");
        }

        public Int32 AddressId
        {
            get { return _addressId;  }
            set { _addressId = value;  }
        }

        public string Address
        {
            get { return _address;  }
            set { _address = value; }
        }

        public Int32 Port
        {
            get { return _port; }
            set { _port = value; }            
        }

        public int TimeToLive
        {
            get { return _timeToLive; }
            set { _timeToLive = value; }
        }

        public Guid SubscriberId
        {
            get { return _subscriberId; }
            set { _subscriberId = value; }
        }

        public byte[] Package
        {
            get { return _package; }
            set { _package = value; }
        }

        public IEventMessage EventMessage
        {
            get { return _eventMessage; }
            set { _eventMessage = value; }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("SubscriberId", _subscriberId, _subscriberId.GetType());
            info.AddValue("AddressId", _addressId, _addressId.GetType());
            info.AddValue("Address", _address, _address.GetType());
            info.AddValue("Port", _port, _port.GetType());
            info.AddValue("TimeToLive", _timeToLive, _timeToLive.GetType());
        }

    }
}
