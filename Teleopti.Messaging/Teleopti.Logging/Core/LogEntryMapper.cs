using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Logging.Core
{
    public class EventLogEntryMapper : MapperBase<ILogEntry>
    {
        public EventLogEntryMapper(string connectionString) : base(connectionString)
        {
        }

        protected override ILogEntry Map(IDataRecord record)
        {
            ILogEntry eventLogEntry = new LogEntry();
            eventLogEntry.LogId = (DBNull.Value == record["LogId"]) ? Guid.Empty : (Guid)record["LogId"];
            eventLogEntry.ProcessId = (DBNull.Value == record["ProcessId"]) ? 0 : (int)record["ProcessId"];
            eventLogEntry.Description = (DBNull.Value == record["Description"]) ? string.Empty : (string)record["Description"];
            eventLogEntry.Exception = (DBNull.Value == record["Exception"]) ? string.Empty : (string)record["Exception"];
            eventLogEntry.Message = (DBNull.Value == record["Message"]) ? string.Empty : (string)record["Message"];
            eventLogEntry.StackTrace = (DBNull.Value == record["StackTrace"]) ? string.Empty : (string)record["StackTrace"];
            eventLogEntry.ChangedBy = (DBNull.Value == record["ChangedBy"]) ? string.Empty : (string)record["ChangedBy"];
            eventLogEntry.ChangedDateTime = (DBNull.Value == record["ChangedDateTime"]) ? Consts.MinDate : (DateTime)record["ChangedDateTime"];
            return eventLogEntry;
        }

        public override bool Insert(SqlConnection connection, ILogEntry domainObject)
        {
            SqlParameter[] paramsToStore = new SqlParameter[8];
            paramsToStore[0] = new SqlParameter("@LogId", SqlDbType.UniqueIdentifier);
            paramsToStore[0].Value = domainObject.LogId;
            paramsToStore[0].Direction = ParameterDirection.InputOutput;
            paramsToStore[1] = new SqlParameter("@ProcessId", SqlDbType.Int);
            paramsToStore[1].Value = domainObject.ProcessId;
            paramsToStore[2] = new SqlParameter("@Description", SqlDbType.Text);
            paramsToStore[2].Value = domainObject.Description;
            paramsToStore[3] = new SqlParameter("@Exception", SqlDbType.Text);
            paramsToStore[3].Value = domainObject.Exception;
            paramsToStore[4] = new SqlParameter("@Message", SqlDbType.Text);
            paramsToStore[4].Value = domainObject.Message;
            paramsToStore[5] = new SqlParameter("@StackTrace", SqlDbType.Text);
            paramsToStore[5].Value = domainObject.StackTrace;
            paramsToStore[6] = new SqlParameter("@ChangedBy", SqlDbType.NVarChar);
            paramsToStore[6].Value = (domainObject.ChangedBy.Length > 10 ? domainObject.ChangedBy.Substring(0, 10) : domainObject.ChangedBy);
            paramsToStore[6].Size = 10;
            paramsToStore[7] = new SqlParameter("@ChangedDateTime", SqlDbType.DateTime);
            paramsToStore[7].Value = domainObject.ChangedDateTime;
            ExecuteNonQuery(connection, CommandType.StoredProcedure, "msg.sp_Log_Insert", paramsToStore);
            if (domainObject.LogId == Guid.Empty)
                domainObject.LogId = (Guid)paramsToStore[0].Value;  
            return true;
        }

        
    }
}