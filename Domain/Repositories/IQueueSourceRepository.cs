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
        /// This is to get the distinct log item names
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        IDictionary<int, string> GetDistinctLogItemName();
    }
}