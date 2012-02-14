using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// Interface for QueueSourceRepository
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-05-07
    /// </remarks>
    public interface IQueueSourceRepository : IRepository<IQueueSource>
    {
        /// <summary>
        /// Loads all queues.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-07
        /// </remarks>
        IList<IQueueSource> LoadAllQueues();
    }
}