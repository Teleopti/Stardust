using System;
using System.Diagnostics;
using Teleopti.Core;
using Teleopti.Messaging.Exceptions;
using log4net;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class HeartbeatDeleter : ObjectDeleter
    {
        private const string HeartbeatDeleteAll = "msg.sp_Heartbeat_Delete_All";
		private static ILog Logger = LogManager.GetLogger(typeof(HeartbeatDeleter));

        public HeartbeatDeleter(string connectionString) : base(connectionString)
        {
        }

        public void DeleteHeartbeats()
        {
            try
            {
                DeleteRecords(HeartbeatDeleteAll);
                Logger.Warn("Deleted all heartbeats to clear up database.");
            }
            catch (Exception exception)
            {
                Logger.Error("Heartbeat deleter error.", exception);
                throw new DatabaseException("DeleteHeartbeats()", exception);
            }
        }

    }
}
