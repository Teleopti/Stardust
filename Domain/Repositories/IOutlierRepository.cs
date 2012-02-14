using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// Interface for OutlierRepository
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-05-16
    /// </remarks>
    public interface IOutlierRepository : IRepository<IOutlier>
    {
        /// <summary>
        /// Finds outliers by workload.
        /// </summary>
        /// <param name="workload">The workload.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-16
        /// </remarks>
        IList<IOutlier> FindByWorkload(IWorkload workload);
    }
}
