using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Messaging.Core
{
    [Serializable]
    public class MulticastAddressInfo : IMulticastAddressInfo
    {
        private Int32 _multicastAddressId;
        private string _multicastAddress;
        private Int32 _port;
        private BroadcastDirection _direction;

        public MulticastAddressInfo()
        {

        }

        protected MulticastAddressInfo(SerializationInfo info, StreamingContext context)
        {
            _multicastAddressId = info.GetInt32("MulticastAddressId");
            _multicastAddress = info.GetString("MulticastAddress");
            _port = info.GetInt32("Port");
            _direction = (BroadcastDirection) Enum.Parse(typeof(BroadcastDirection), info.GetString("Direction"));
        }

        public MulticastAddressInfo(Int32 multicastAddressId, string multicastAddress, Int32 port, BroadcastDirection direction)
        {
            _multicastAddressId = multicastAddressId;
            _multicastAddress = multicastAddress;
            _port = port;
            _direction = direction;
        }

        public Int32 MulticastAddressId
        {
            get { return _multicastAddressId;  }
            set { _multicastAddressId = value;  }
        }

        public string MulticastAddress
        {
            get { return _multicastAddress;  }
            set { _multicastAddress = value; }
        }

        public Int32 Port
        {
            get { return _port; }
            set { _port = value; }            
        }

        public BroadcastDirection BroadcastDirection
        {
            get { return _direction;  }
            set { _direction = value; }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("MulticastAddressId", _multicastAddressId, _multicastAddressId.GetType());
            info.AddValue("MulticastAddress", _multicastAddress, _multicastAddress.GetType());
            info.AddValue("Port", _port, _port.GetType());
            info.AddValue("Direction", _direction.ToString());
        }

    }
}
