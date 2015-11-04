using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// General interface for entities that are members of the Business Unit Hierarchy
    /// </summary>
    public interface IBusinessUnitHierarchyEntity : IEntity
    {
        /// <summary>
        /// Collects all the persons in the business unit hierarchy entity at the given datetime.
        /// </summary>
        /// <param name="candidates">The candidates who can be in the hierarchy.</param>
        /// <param name="period">The period.</param>
        /// <returns>All persons in the hierarchy.</returns>
        ReadOnlyCollection<IPerson> PersonsInHierarchy(IEnumerable<IPerson> candidates, DateOnlyPeriod period);
    
    }
}