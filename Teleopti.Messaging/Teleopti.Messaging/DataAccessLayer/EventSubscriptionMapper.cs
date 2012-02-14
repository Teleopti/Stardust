using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class EventSubscriberMapper : MapperBase<IEventSubscriber>
    {

        public EventSubscriberMapper(string connectionString) : base(connectionString)
        {
        }

        protected override IEventSubscriber Map(IDataRecord record)
        {
            IEventSubscriber eventSubscriber = new EventSubscriber();
            eventSubscriber.SubscriberId = (DBNull.Value == record["SubscriberId"]) ? Guid.Empty : (Guid) record["SubscriberId"];
            eventSubscriber.UserId = (DBNull.Value == record["UserId"]) ? 0 : (Int32) record["UserId"];
            eventSubscriber.ProcessId = (DBNull.Value == record["ProcessId"]) ? 0 : (Int32)record["ProcessId"];
            eventSubscriber.IPAddress = (DBNull.Value == record["IPAddress"]) ? String.Empty : (String)record["IPAddress"];
            eventSubscriber.Port = (DBNull.Value == record["Port"]) ? 0 : (Int32)record["Port"];
            eventSubscriber.ChangedBy = (DBNull.Value == record["ChangedBy"]) ? string.Empty : (string)record["ChangedBy"];
            eventSubscriber.ChangedDateTime = (DBNull.Value == record["ChangedDateTime"]) ? Consts.MinDate : (DateTime)record["ChangedDateTime"];
            return eventSubscriber;
        }

        public override bool Insert(SqlConnection connection, IEventSubscriber domainObject)
        {
            SqlParameter[] paramsToStore = new SqlParameter[7];
            paramsToStore[0] = new SqlParameter("@SubscriberId", SqlDbType.UniqueIdentifier);
            paramsToStore[0].Value = domainObject.SubscriberId;
            paramsToStore[0].Direction = ParameterDirection.InputOutput;
            paramsToStore[1] = new SqlParameter("@UserId", SqlDbType.Int);
            paramsToStore[1].Value = domainObject.UserId;
            paramsToStore[2] = new SqlParameter("@ProcessId", SqlDbType.NVarChar);
            paramsToStore[2].Value = domainObject.ProcessId;
            paramsToStore[2].Size = 30;
            paramsToStore[3] = new SqlParameter("@IPAddress", SqlDbType.NVarChar);
            paramsToStore[3].Size = 15;
            paramsToStore[3].Value = domainObject.IPAddress;
            paramsToStore[4] = new SqlParameter("@Port", SqlDbType.Int);
            paramsToStore[4].Value = domainObject.Port;
            paramsToStore[5] = new SqlParameter("@ChangedBy", SqlDbType.NVarChar);
            paramsToStore[5].Value = (domainObject.ChangedBy.Length > 10 ? domainObject.ChangedBy.Substring(0, 10) : domainObject.ChangedBy);
            paramsToStore[5].Size = 10;
            paramsToStore[6] = new SqlParameter("@ChangedDateTime", SqlDbType.DateTime);
            paramsToStore[6].Value = domainObject.ChangedDateTime;
            ExecuteNonQuery(connection, CommandType.StoredProcedure, "msg.sp_Subscriber_Insert", paramsToStore);
            if (domainObject.SubscriberId == Guid.Empty)
                domainObject.SubscriberId = (Guid)(paramsToStore[0].Value);    
            return true;
        }

    }
}