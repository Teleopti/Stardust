using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class EventFilterMapper : MapperBase<IEventFilter>
    {
        public EventFilterMapper(string connectionString) : base(connectionString)
        {
        }

        protected override IEventFilter Map(IDataRecord record)
        {
            IEventFilter eventFilter = new EventFilter();
            eventFilter.FilterId = (DBNull.Value == record["FilterId"]) ? Guid.Empty : (Guid)record["FilterId"];
            eventFilter.SubscriberId = (DBNull.Value == record["SubscriberId"]) ? Guid.Empty : (Guid)record["SubscriberId"];
            eventFilter.ReferenceObjectId = (DBNull.Value == record["ReferenceObjectId"]) ? Guid.Empty : (Guid)record["ReferenceObjectId"];
            eventFilter.ReferenceObjectType = (DBNull.Value == record["ReferenceObjectType"]) ? string.Empty : (string)record["ReferenceObjectType"];
            eventFilter.DomainObjectId = (DBNull.Value == record["DomainObjectId"]) ? Guid.Empty : (Guid)record["DomainObjectId"];
            eventFilter.DomainObjectType = (DBNull.Value == record["DomainObjectType"]) ? string.Empty : (string)record["DomainObjectType"];
            eventFilter.EventStartDate = (DBNull.Value == record["EventStartDate"]) ? Consts.MinDate : (DateTime)record["EventStartDate"];
            eventFilter.EventEndDate = (DBNull.Value == record["EventEndDate"]) ? Consts.MinDate : (DateTime)record["EventEndDate"];
            eventFilter.ChangedBy = (DBNull.Value == record["ChangedBy"]) ? string.Empty : (string)record["ChangedBy"];
            eventFilter.ChangedDateTime = (DBNull.Value == record["ChangedDateTime"]) ? Consts.MinDate : (DateTime)record["ChangedDateTime"];
            return eventFilter;
        }

        public override bool Insert(SqlConnection connection, IEventFilter domainObject)
        {
            SqlParameter[] paramsToStore = new SqlParameter[10];
            paramsToStore[0] = new SqlParameter("@FilterId", SqlDbType.UniqueIdentifier);
            paramsToStore[0].Value = domainObject.FilterId;
            paramsToStore[0].Direction = ParameterDirection.InputOutput;
            paramsToStore[1] = new SqlParameter("@SubscriberId", SqlDbType.UniqueIdentifier);
            paramsToStore[1].Value = domainObject.SubscriberId;
            paramsToStore[2] = new SqlParameter("@ReferenceObjectId", SqlDbType.UniqueIdentifier);
            paramsToStore[2].Value = domainObject.ReferenceObjectId;
            paramsToStore[3] = new SqlParameter("@ReferenceObjectType", SqlDbType.NVarChar);
            paramsToStore[3].Value = domainObject.ReferenceObjectType;
            paramsToStore[3].Size = 255;
            paramsToStore[4] = new SqlParameter("@DomainObjectId", SqlDbType.UniqueIdentifier);
            paramsToStore[4].Value = domainObject.DomainObjectId;
            paramsToStore[5] = new SqlParameter("@DomainObjectType", SqlDbType.NVarChar);
            paramsToStore[5].Value = domainObject.DomainObjectType;
            paramsToStore[5].Size = 255;
            paramsToStore[6] = new SqlParameter("@EventStartDate", SqlDbType.DateTime);
            paramsToStore[6].Value = domainObject.EventStartDate;
            paramsToStore[7] = new SqlParameter("@EventEndDate", SqlDbType.DateTime);
            paramsToStore[7].Value = domainObject.EventEndDate;
            paramsToStore[8] = new SqlParameter("@ChangedBy", SqlDbType.NVarChar);
            paramsToStore[8].Value = (domainObject.ChangedBy.Length > 10 ? domainObject.ChangedBy.Substring(0, 10) : domainObject.ChangedBy);
            paramsToStore[8].Size = 10;
            paramsToStore[9] = new SqlParameter("@ChangedDateTime", SqlDbType.DateTime);
            paramsToStore[9].Value = domainObject.ChangedDateTime;
            ExecuteNonQuery(connection, CommandType.StoredProcedure, "msg.sp_Filter_Insert", paramsToStore);
            if (domainObject.FilterId == Guid.Empty)
                domainObject.FilterId = (Guid)(paramsToStore[0].Value);    
            return true;
        }

    }
}