using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class HeartbeatMapper : MapperBase<IEventHeartbeat>
    {

        public HeartbeatMapper(string connectionString) : base(connectionString)
        {
        }

        public override bool Insert(SqlConnection connection, IEventHeartbeat domainObject)
        {
            SqlParameter[] paramsToStore = new SqlParameter[5];
            paramsToStore[0] = new SqlParameter("@HeartbeatId", SqlDbType.UniqueIdentifier);
            paramsToStore[0].Value = domainObject.HeartbeatId;
            paramsToStore[0].Direction = ParameterDirection.InputOutput;
            paramsToStore[1] = new SqlParameter("@SubscriberId", SqlDbType.UniqueIdentifier);
            paramsToStore[1].Value = domainObject.SubscriberId;
            paramsToStore[2] = new SqlParameter("@ProcessId", SqlDbType.Int);
            paramsToStore[2].Value = domainObject.ProcessId;
            paramsToStore[3] = new SqlParameter("@ChangedBy", SqlDbType.NVarChar);
            paramsToStore[3].Value = (domainObject.ChangedBy.Length > 10 ? domainObject.ChangedBy.Substring(0, 10) : domainObject.ChangedBy);
            paramsToStore[3].Size = 10;
            paramsToStore[4] = new SqlParameter("@ChangedDateTime", SqlDbType.DateTime);
            paramsToStore[4].Value = domainObject.ChangedDateTime;
            ExecuteNonQuery(connection, CommandType.StoredProcedure, "msg.sp_Heartbeat_Insert", paramsToStore);
            if (domainObject.HeartbeatId == Guid.Empty)
                domainObject.HeartbeatId = (Guid)(paramsToStore[0].Value);  
            return true;

        }   

        protected override IEventHeartbeat Map(IDataRecord record)
        {
            IEventHeartbeat eventHeartbeat = new EventHeartbeat();
            eventHeartbeat.HeartbeatId = (DBNull.Value == record["HeartbeatId"]) ? Guid.Empty : new Guid(record["HeartbeatId"].ToString()); 
            eventHeartbeat.SubscriberId = (DBNull.Value == record["SubscriberId"]) ? Guid.Empty : (Guid)record["SubscriberId"];
            eventHeartbeat.ProcessId = (DBNull.Value == record["ProcessId"]) ? 0 : (int)record["ProcessId"];
            eventHeartbeat.ChangedBy = (DBNull.Value == record["ChangedBy"]) ? string.Empty : (string)record["ChangedBy"];
            eventHeartbeat.ChangedDateTime = (DBNull.Value == record["ChangedDateTime"]) ? Consts.MinDate : (DateTime)record["ChangedDateTime"];
            return eventHeartbeat;
        }

    }
}
