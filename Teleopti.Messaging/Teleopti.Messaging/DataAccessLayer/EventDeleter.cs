using System;
using System.Diagnostics;
using Teleopti.Core;
using Teleopti.Logging;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class EventDeleter : ObjectDeleter
    {
        private const string CommandText = "msg.sp_Scavage_Events";

        public EventDeleter(string connectionString) : base(connectionString)
        {
        }

        public void DeleteEvent()
        {
            try
            {
                DeleteRecords(CommandText);
            }
            catch (Exception exc)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.Message + exc.StackTrace);
                throw new DatabaseException("DeleteEvent()", exc);
            }
        }


    }
}
