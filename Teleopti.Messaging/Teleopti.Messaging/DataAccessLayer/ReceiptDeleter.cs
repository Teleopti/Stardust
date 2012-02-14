using System;
using System.Diagnostics;
using Teleopti.Core;
using Teleopti.Logging;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class ReceiptDeleter : ObjectDeleter
    {
        private const string ReceiptDelete = "msg.sp_Receipt_Delete_All";

        public ReceiptDeleter(string connectionString) : base(connectionString)
        {
        }

        public void DeleteReceipts()
        {
            try
            {
                DeleteRecords(ReceiptDelete);
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Deleted all receipts to clear up database.");
            }
            catch (Exception exception)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exception.ToString());
                throw new DatabaseException("DeleteReceipts()", exception);
            }
        }

    }
}
