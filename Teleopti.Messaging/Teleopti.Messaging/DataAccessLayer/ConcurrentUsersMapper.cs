using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Client;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class ConcurrentUsersMapper : MapperBase<IConcurrentUsers>
    {

        public ConcurrentUsersMapper(string connectionString) : base(connectionString)
        {
        }

        public override bool Insert(SqlConnection connection, IConcurrentUsers domainObject)
        {
            throw new NotImplementedException();
        }

        protected override IConcurrentUsers Map(IDataRecord record)
        {
            IConcurrentUsers addressInfo = new ConcurrentUsers();
            addressInfo.IPAddress = (DBNull.Value == record["IpAddress"]) ? string.Empty : (string)record["IpAddress"];
            addressInfo.NumberOfConcurrentUsers = (DBNull.Value == record["NumberOfConcurrentUsers"]) ? 0 : (int)record["NumberOfConcurrentUsers"];
            return addressInfo;
        }

    }
}
