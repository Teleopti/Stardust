using System;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.Exceptions;
using log4net;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class ReceiptDeleter : ObjectDeleter
    {
    	private static ILog Logger = LogManager.GetLogger(typeof (ReceiptDeleter));
        private const string ReceiptDelete = "msg.sp_Receipt_Delete_All";

        public ReceiptDeleter(string connectionString) : base(connectionString)
        {
        }

        public void DeleteReceipts()
        {
            try
            {
                DeleteRecords(ReceiptDelete);
                Logger.Warn("Deleted all receipts to clear up database.");
            }
            catch (Exception exception)
            {
                Logger.Error("Delete receipts error.", exception);
                throw new DatabaseException("DeleteReceipts()", exception);
            }
        }

    }
}
