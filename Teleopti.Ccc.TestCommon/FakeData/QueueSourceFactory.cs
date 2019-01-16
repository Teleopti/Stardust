using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Queuesource factory
    /// </summary>
    public static class QueueSourceFactory
    {
        /// <summary>
        /// Creates a Queuesource 
        /// </summary>
        /// <returns></returns>
        public static QueueSource CreateQueueSource()
        {
            string name = "Name of Queue";
            string description = "Description of the Queue";
            string ctiQueueId = "17";

            return new QueueSource(name, description, ctiQueueId);
        }

        /// <summary>
        /// Creates the queue source inrikes.
        /// </summary>
        /// <returns></returns>
        public static QueueSource CreateQueueSourceInrikes()
        {
            string name = "Inrikes";
            string description = "Inrikessamtal";
            string ctiQueueId = "178";

            return new QueueSource(name, description, ctiQueueId);
        }

        /// <summary>
        /// Creates the queue source help desk.
        /// </summary>
        /// <returns></returns>
        public static QueueSource CreateQueueSourceHelpdesk()
        {
            string name = "HelpDesk";
            string description = "Helpdesk-samtal";
            string ctiQueueId = "228";

            return new QueueSource(name, description, ctiQueueId);
        }
    }
}