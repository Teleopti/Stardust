using System;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Core
{

    /// <summary>
    /// The Polling Managers interface.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 15/05/2010
    /// </remarks>
    public interface IPollingManager
    {
        /// <summary>
        /// Adds the client.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        void AddClient(Guid subscriberId);

        /// <summary>
        /// Removes the client.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        void RemoveClient(Guid subscriberId);

        /// <summary>
        /// Queues the event message for sending.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <param name="eventMessage">The event message.</param>
        void QueueMessage(Guid subscriberId, IEventMessage eventMessage);

        /// <summary>
        /// Polls the specified subscriber's queue.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        IEventMessage[] Poll(Guid subscriberId);

    }
}