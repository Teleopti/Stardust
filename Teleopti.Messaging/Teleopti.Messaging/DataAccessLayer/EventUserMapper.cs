using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class EventUserMapper : MapperBase<IEventUser>
    {

        public EventUserMapper(string connectionString) : base(connectionString)
        {
        }

        protected override IEventUser Map(IDataRecord record)
        {
            IEventUser eventUser = new EventUser();
            eventUser.UserId = (DBNull.Value == record["UserId"]) ? 0 : (int)record["UserId"];
            eventUser.Domain = (DBNull.Value == record["Domain"]) ? string.Empty : (string)record["Domain"];
            eventUser.UserName = (DBNull.Value == record["UserName"]) ? string.Empty : (string)record["UserName"];
            eventUser.ChangedBy = (DBNull.Value == record["ChangedBy"]) ? string.Empty : (string)record["ChangedBy"];
            eventUser.ChangedDateTime = (DBNull.Value == record["ChangedDateTime"]) ? Consts.MinDate : (DateTime)record["ChangedDateTime"];
            return eventUser;
        }

        public override bool Insert(SqlConnection connection, IEventUser domainObject)
        {
            SqlParameter[] paramsToStore = new SqlParameter[5];
            paramsToStore[0] = new SqlParameter("@UserId", SqlDbType.Int);
            paramsToStore[0].Direction = ParameterDirection.InputOutput;
            paramsToStore[0].Value = domainObject.UserId;
            paramsToStore[1] = new SqlParameter("@Domain", SqlDbType.NVarChar);
            paramsToStore[1].Value = domainObject.Domain;
            paramsToStore[1].Size = 50;
            paramsToStore[2] = new SqlParameter("@UserName", SqlDbType.NVarChar);
            paramsToStore[2].Value = domainObject.UserName;
            paramsToStore[2].Size = 50;
            paramsToStore[3] = new SqlParameter("@ChangedBy", SqlDbType.NVarChar);
            paramsToStore[3].Value = (domainObject.ChangedBy.Length > 10 ? domainObject.ChangedBy.Substring(0, 10) : domainObject.ChangedBy);
            paramsToStore[3].Size = 10;
            paramsToStore[4] = new SqlParameter("@ChangedDateTime", SqlDbType.DateTime);
            paramsToStore[4].Value = domainObject.ChangedDateTime;
            ExecuteNonQuery(connection, CommandType.StoredProcedure, "msg.sp_Users_Insert", paramsToStore);
            if (domainObject.UserId == 0)
                domainObject.UserId = (int) paramsToStore[0].Value;
            return true;
        }

    }
}