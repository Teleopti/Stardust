using System;
using Teleopti.Core;
using Teleopti.Messaging.Exceptions;
using log4net;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class EventDeleter : ObjectDeleter
    {
		private static ILog Logger = LogManager.GetLogger(typeof(EventDeleter));
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
                Logger.Error("Event deleter error.", exc);
                throw new DatabaseException("DeleteEvent()", exc);
            }
        }


    }
}
