using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// Contract schedule repository
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-10-22
    /// </remarks>
    public interface IContractScheduleRepository : IRepository<IContractSchedule>
    {
        /// <summary>
        /// Finds all contract schedule by description.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 3/31/2008
        /// </remarks>
        ICollection<IContractSchedule> FindAllContractScheduleByDescription();

        /// <summary>
        /// Loads all aggregate.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-10-22
        /// </remarks>
        ICollection<IContractSchedule> LoadAllAggregate();
    }
}