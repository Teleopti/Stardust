using System;
using System.Diagnostics;
using Teleopti.Core;
using Teleopti.Logging;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class SubscriberDeleter : ObjectDeleter
    {
        private const string SubscriberIdParameter = "@SubscriberId";
        private const string DeleteSubscriberById = "msg.sp_Subscriber_Delete";

        public SubscriberDeleter(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Deletes subscription from the database.
        /// </summary>
        /// <param name="subscriberId"></param>
        public void UnregisterSubscription(Guid subscriberId)
        {
            try
            {
                DeleteAddedRecord(DeleteSubscriberById, SubscriberIdParameter, subscriberId);
            }
            catch (Exception exception)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exception.Message + exception.StackTrace);
                throw new DatabaseException("UnregisterSubscription(Guid subscriberId)", exception);
            }
        }

    }
}
