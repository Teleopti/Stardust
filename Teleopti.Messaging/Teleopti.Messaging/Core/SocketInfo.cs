using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Messaging.Core
{
    [Serializable]
    public class SocketInfo : ISocketInfo
    {
        [NonSerialized]
        private IPAddress _ipAddress;
        [NonSerialized]
        private Socket _socket;
        [NonSerialized]
        private IPEndPoint _ipEndPoint;

        private string _address;
        private int _port;
        private int _timeToLive = 1;
        private int _clientThrottle = 50;

        public SocketInfo(string address, int port, int clientThrottle)
        {
            _address = address;
            _port = port;
            _clientThrottle = clientThrottle;
        }

        protected SocketInfo(SerializationInfo info, StreamingContext context)
        {
            _address = info.GetString("Address");
            _port = info.GetInt32("Port");
            _timeToLive = info.GetInt32("TimeToLive");
            _clientThrottle = info.GetInt32("ClientThrottle");
        }

        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public int TimeToLive
        {
            get { return _timeToLive; }
            set { _timeToLive = value; }            
        }

        public int ClientThrottle
        {
            get { return _clientThrottle; }
            set { _clientThrottle = value; }
        }

        public IPAddress IPAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }            
        }

        public Socket Socket
        {
            get { return _socket; }
            set { _socket = value; }
        }

        public IPEndPoint IPEndpoint
        {
            get { return _ipEndPoint; }
            set { _ipEndPoint = value; }
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Address", _address);
            info.AddValue("Port", _port);
            info.AddValue("TimeToLive", _timeToLive);
            info.AddValue("ClientThrottle",_clientThrottle);
        }
    }
}
