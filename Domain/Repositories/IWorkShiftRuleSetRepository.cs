using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// Work shift rule set repository
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2009-12-08
    /// </remarks>
    public interface IWorkShiftRuleSetRepository : IRepository<IWorkShiftRuleSet>
    {
        /// <summary>
        /// Finds all with limiters and extenders included.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-15
        /// </remarks>
        ICollection<IWorkShiftRuleSet> FindAllWithLimitersAndExtenders();
    }
}