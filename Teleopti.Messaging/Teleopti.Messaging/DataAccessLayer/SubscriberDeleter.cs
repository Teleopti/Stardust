using System;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.Exceptions;
using log4net;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class SubscriberDeleter : ObjectDeleter
    {
		private static ILog Logger = LogManager.GetLogger(typeof(SubscriberDeleter));
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
                Logger.Error("Unregister subscription error.", exception);
                throw new DatabaseException("UnregisterSubscription(Guid subscriberId)", exception);
            }
        }

    }
}
