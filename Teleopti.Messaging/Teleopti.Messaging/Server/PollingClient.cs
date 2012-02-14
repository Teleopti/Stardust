using System;
using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Server
{
    public class PollingClient : IDisposable
    {
        private Queue<IEventMessage> _queue = new Queue<IEventMessage>();
        private static object _lockObject = new object();
        private Guid _subscriberId;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PollingClient"/> class.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        public PollingClient(Guid subscriberId)
        {
            _subscriberId = subscriberId;
        }

        /// <summary>
        /// Gets the subscriber id.
        /// </summary>
        /// <value>The subscriber id.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        public Guid SubscriberId
        {
            get { return _subscriberId; }
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/05/2010
        /// </remarks>
        public IEventMessage[] RetrieveMessages()
        {
            IEventMessage[] eventMessageArray = null;
            lock (_lockObject)
            {
                if(_queue.Count > 0)
                {
                    List<IEventMessage> eventMessages = new List<IEventMessage>();
                    eventMessages.AddRange(_queue.ToArray());
                    eventMessages.Sort();
                    eventMessageArray = eventMessages.ToArray();
                    _queue.Clear();
                }
            }
            return eventMessageArray;
        }

        /// <summary>
        /// Queues the item.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/05/2010
        /// </remarks>
        public void QueueItem(IEventMessage eventMessage)
        {
            lock (_lockObject)
            {
                _queue.Enqueue(eventMessage);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                lock (_lockObject)
                {
                    _queue.Clear();
                    _queue = null;
                }
            }
        }
    }
}