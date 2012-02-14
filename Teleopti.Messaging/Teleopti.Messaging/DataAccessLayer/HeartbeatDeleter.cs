using System;
using System.Diagnostics;
using Teleopti.Core;
using Teleopti.Logging;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class HeartbeatDeleter : ObjectDeleter
    {
        private const string HeartbeatDeleteAll = "msg.sp_Heartbeat_Delete_All";

        public HeartbeatDeleter(string connectionString) : base(connectionString)
        {
        }

        public void DeleteHeartbeats()
        {
            try
            {
                DeleteRecords(HeartbeatDeleteAll);
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Deleted all heartbeats to clear up database.");
            }
            catch (Exception exception)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exception.ToString());
                throw new DatabaseException("DeleteHeartbeats()", exception);
            }
        }

    }
}
