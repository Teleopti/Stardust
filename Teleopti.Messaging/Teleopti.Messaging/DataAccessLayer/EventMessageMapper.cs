using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class EventMessageMapper : MapperBase<IEventMessage>
    {
        public EventMessageMapper(string connectionString) : base(connectionString)
        {
        }

        protected override IEventMessage Map(IDataRecord record)
        {
            IEventMessage eventMessage = new EventMessage();
            eventMessage.EventId = (DBNull.Value == record["EventId"]) ? Guid.Empty : (Guid)record["EventId"];
            eventMessage.EventStartDate = (DBNull.Value == record["StartDate"]) ? Consts.MinDate : (DateTime)record["StartDate"];
            eventMessage.EventEndDate = (DBNull.Value == record["EndDate"]) ? Consts.MaxDate : (DateTime)record["EndDate"];
            eventMessage.UserId = (DBNull.Value == record["UserId"]) ? 0 : (int)record["UserId"];
            eventMessage.ProcessId = (DBNull.Value == record["ProcessId"]) ? 0 : (int)record["ProcessId"];
            eventMessage.ModuleId = (DBNull.Value == record["ModuleId"]) ? Guid.Empty : (Guid)record["ModuleId"];
            eventMessage.PackageSize = (DBNull.Value == record["PackageSize"]) ? 0 : (int)record["PackageSize"];
            eventMessage.IsHeartbeat = (DBNull.Value == record["IsHeartbeat"]) ? false : (bool)record["IsHeartbeat"];
            eventMessage.ReferenceObjectId = (DBNull.Value == record["ReferenceObjectId"]) ? Guid.Empty : (Guid)record["ReferenceObjectId"];
            eventMessage.ReferenceObjectType = (DBNull.Value == record["ReferenceObjectType"]) ? string.Empty : (String)record["ReferenceObjectType"];
            eventMessage.DomainObjectId = (DBNull.Value == record["DomainObjectId"]) ? Guid.Empty : (Guid)record["DomainObjectId"];
            eventMessage.DomainObjectType = (DBNull.Value == record["DomainObjectType"]) ? string.Empty : (String)record["DomainObjectType"];
            eventMessage.DomainUpdateType = (DomainUpdateType) ((DBNull.Value == record["DomainUpdateType"]) ? 0 : (int)record["DomainUpdateType"] );
            eventMessage.DomainObject = (DBNull.Value == record["DomainObject"]) ? new byte[0] : (byte[])record["DomainObject"];
            eventMessage.ChangedBy = (DBNull.Value == record["ChangedBy"]) ? string.Empty : (string)record["ChangedBy"];
            eventMessage.ChangedDateTime = (DBNull.Value == record["ChangedDateTime"]) ? Consts.MinDate : (DateTime)record["ChangedDateTime"];
            return eventMessage;
        }

        public override bool Insert(SqlConnection connection, IEventMessage domainObject)
        {
            SqlParameter[] paramsToStore = new SqlParameter[16];
            paramsToStore[0] = new SqlParameter("@EventId", SqlDbType.UniqueIdentifier);
            paramsToStore[0].Value = domainObject.EventId;
            paramsToStore[0].Direction = ParameterDirection.InputOutput;
            paramsToStore[1] = new SqlParameter("@StartDate", SqlDbType.DateTime);
            paramsToStore[1].Value = domainObject.EventStartDate;
            paramsToStore[2] = new SqlParameter("@EndDate", SqlDbType.DateTime);
            paramsToStore[2].Value = domainObject.EventEndDate;
            paramsToStore[3] = new SqlParameter("@UserId", SqlDbType.Int);
            paramsToStore[3].Value = domainObject.UserId;
            paramsToStore[4] = new SqlParameter("@ProcessId", SqlDbType.Int);
            paramsToStore[4].Value = domainObject.ProcessId;
            paramsToStore[5] = new SqlParameter("@ModuleId", SqlDbType.UniqueIdentifier);
            paramsToStore[5].Value = domainObject.ModuleId;
            paramsToStore[6] = new SqlParameter("@PackageSize", SqlDbType.Int);
            paramsToStore[6].Value = domainObject.PackageSize;
            paramsToStore[7] = new SqlParameter("@IsHeartbeat", SqlDbType.Int);
            paramsToStore[7].Value = domainObject.IsHeartbeat;
            paramsToStore[8] = new SqlParameter("@ReferenceObjectId", SqlDbType.UniqueIdentifier);
            paramsToStore[8].Value = domainObject.ReferenceObjectId;
            paramsToStore[9] = new SqlParameter("@ReferenceObjectType", SqlDbType.NVarChar);
            paramsToStore[9].Value = domainObject.ReferenceObjectType;
            paramsToStore[9].Size = 255;
            paramsToStore[10] = new SqlParameter("@DomainObjectId", SqlDbType.UniqueIdentifier);
            paramsToStore[10].Value = domainObject.DomainObjectId;
            paramsToStore[11] = new SqlParameter("@DomainObjectType", SqlDbType.NVarChar);
            paramsToStore[11].Value = domainObject.DomainObjectType;
            paramsToStore[11].Size = 255;
            paramsToStore[12] = new SqlParameter("@DomainUpdateType", SqlDbType.Int);
            paramsToStore[12].Value = (Int32) domainObject.DomainUpdateType;
            paramsToStore[13] = new SqlParameter("@DomainObject", SqlDbType.VarBinary);
            paramsToStore[13].Value = domainObject.DomainObject;
            paramsToStore[14] = new SqlParameter("@ChangedBy", SqlDbType.NVarChar);
            paramsToStore[14].Value = (domainObject.ChangedBy.Length > 10 ? domainObject.ChangedBy.Substring(0, 10) : domainObject.ChangedBy);
            paramsToStore[14].Size = 10;
            paramsToStore[15] = new SqlParameter("@ChangedDateTime", SqlDbType.DateTime);
            paramsToStore[15].Value = domainObject.ChangedDateTime;
            ExecuteNonQuery(connection, CommandType.StoredProcedure, "msg.sp_Event_Insert", paramsToStore);
            if (domainObject.EventId == Guid.Empty)
                domainObject.EventId = (Guid)paramsToStore[0].Value;  
            return true;
        }

    }
}