using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// General interface for entities that are members of the Business Unit Hierarchy
    /// </summary>
    public interface IBusinessUnitHierarchyEntity
    {
        /// <summary>
        /// Collects all the persons in the business unit hierarchy entity at the given datetime.
        /// </summary>
        /// <param name="candidates">The candidates who can be in the hierarchy.</param>
        /// <param name="period">The period.</param>
        /// <returns>All persons in the hierarchy.</returns>
        ReadOnlyCollection<Person> PersonsInHierarchy(ICollection<Person> candidates, DateTimePeriod period);
    
    }
}
