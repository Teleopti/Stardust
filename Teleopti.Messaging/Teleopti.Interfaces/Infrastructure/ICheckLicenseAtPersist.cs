using System.Collections.Generic;

namespace Teleopti.Interfaces.Infrastructure
{
    /// <summary>
    /// NHibernate events uses this to check licensing limitations when persisting entities to the database
    /// </summary>
    /// <remarks>
    /// Created by: Klas
    /// Created date: 2008-12-03
    /// </remarks>
    public interface ICheckLicenseAtPersist
    {
        /// <summary>
        /// Verifies that the specified unit of work does not break any license when the modified roots are persisted.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="modifiedRoots">The modified roots.</param>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-12-03
        /// </remarks>
        void Verify(IUnitOfWork unitOfWork, IEnumerable<IRootChangeInfo> modifiedRoots);
    }
}