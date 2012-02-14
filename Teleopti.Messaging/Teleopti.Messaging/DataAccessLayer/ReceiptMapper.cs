using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class ReceiptMapper : MapperBase<IEventReceipt>
    {
        public ReceiptMapper(string connectionString) : base(connectionString)
        {
        }

        public override bool Insert(SqlConnection connection, IEventReceipt domainObject)
        {
            SqlParameter[] paramsToStore = new SqlParameter[5];
            paramsToStore[0] = new SqlParameter("@ReceiptId", SqlDbType.UniqueIdentifier);
            paramsToStore[0].Value = domainObject.ReceiptId;
            paramsToStore[0].Direction = ParameterDirection.InputOutput;
            paramsToStore[1] = new SqlParameter("@EventId", SqlDbType.UniqueIdentifier);
            paramsToStore[1].Value = domainObject.EventId;
            paramsToStore[2] = new SqlParameter("@ProcessId", SqlDbType.Int);
            paramsToStore[2].Value = domainObject.ProcessId;
            paramsToStore[3] = new SqlParameter("@ChangedBy", SqlDbType.NVarChar);
            paramsToStore[3].Value = (domainObject.ChangedBy.Length > 10 ? domainObject.ChangedBy.Substring(0, 10) : domainObject.ChangedBy);
            paramsToStore[3].Size = 10;
            paramsToStore[4] = new SqlParameter("@ChangedDateTime", SqlDbType.DateTime);
            paramsToStore[4].Value = domainObject.ChangedDateTime;
            ExecuteNonQuery(connection, CommandType.StoredProcedure,"msg.sp_Receipt_Insert", paramsToStore);
            if (domainObject.ReceiptId == Guid.Empty)
                domainObject.ReceiptId = (Guid) paramsToStore[0].Value;  
            return true;
        }

        protected override IEventReceipt Map(IDataRecord record)
        {
            IEventReceipt eventReceipt = new EventReceipt();
            eventReceipt.ReceiptId = (DBNull.Value == record["ReceiptId"]) ? Guid.Empty : (Guid)record["ReceiptId"];
            eventReceipt.EventId = (DBNull.Value == record["EventId"]) ? Guid.Empty : (Guid)record["EventId"];
            eventReceipt.ProcessId = (DBNull.Value == record["ProcessId"]) ? 0 : (int)record["ProcessId"];
            eventReceipt.ChangedBy = (DBNull.Value == record["ChangedBy"]) ? string.Empty : (string)record["ChangedBy"];
            eventReceipt.ChangedDateTime = (DBNull.Value == record["ChangedDateTime"]) ? Consts.MinDate : (DateTime)record["ChangedDateTime"];
            return eventReceipt;
        }
    }
}
