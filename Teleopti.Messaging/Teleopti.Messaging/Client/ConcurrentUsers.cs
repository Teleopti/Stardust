using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Messaging.Client
{
    [Serializable]
    public class ConcurrentUsers : IConcurrentUsers
    {
        private int _numberOfConcurrentUsers;
        private string _ipAddress;

        public ConcurrentUsers()
        {
        }

        protected ConcurrentUsers(SerializationInfo info, StreamingContext context)
        {
            _ipAddress = info.GetString("IpAddress");
            _numberOfConcurrentUsers = info.GetInt32("NumberOfConcurrentUsers");
        }

        public ConcurrentUsers(int numberOfConcurrentUsers, string ipAddress)
        {
            _numberOfConcurrentUsers = numberOfConcurrentUsers;
            _ipAddress = ipAddress;
        }

        public string IPAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }
        }

        public int NumberOfConcurrentUsers
        {
            get { return _numberOfConcurrentUsers; }
            set { _numberOfConcurrentUsers = value; }
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("IpAddress", _ipAddress);
            info.AddValue("NumberOfConcurrentUsers", _numberOfConcurrentUsers);
        }
    }
}
