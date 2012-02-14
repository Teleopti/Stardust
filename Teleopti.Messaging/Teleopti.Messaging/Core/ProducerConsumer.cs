using System.Collections;

namespace Teleopti.Messaging.Core
{
    /// <summary>
    /// Represents a thread safe queue.
    /// </summary>	
    public class ProducerConsumer
    {
        private readonly Queue _queue = Queue.Synchronized(new Queue());

        public ProducerConsumer()
        {
        }

        /// <summary>
        /// Gets the number of items in the queue.
        /// </summary>
        /// <value>
        /// The number of items in the queue.
        /// </value>
        public int Count
        {
            get { return _queue.Count; }
        }

        /// <summary>
        /// Removes and returns the object at the beginning of the queue. 
        /// </summary>
        /// <returns>
        /// The object at the beginning of the queue.
        /// </returns>
        public object Dequeue()
        {
            if(_queue.Count > 0)
                return _queue.Dequeue();
            return null;
        }

        /// <summary>
        /// Adds the object at the end of the queue. 
        /// </summary>		
        public void Enqueue(object queueItem)
        {
            _queue.Enqueue(queueItem);
        }

    }
}