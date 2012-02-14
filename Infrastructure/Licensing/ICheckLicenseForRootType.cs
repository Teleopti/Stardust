#region Imports

using Teleopti.Interfaces.Infrastructure;

#endregion

namespace Teleopti.Ccc.Infrastructure.Licensing
{
    /// <summary>
    /// License checking for particular root type when persisting it to the database
    /// </summary>
    /// <remarks>
    /// Created by: Klas
    /// Created date: 2008-12-03
    /// </remarks>
    public interface ICheckLicenseForRootType
    {
        /// <summary>
        /// Verifies that the specified unit of work does not break any license when called.
        /// </summary>
        /// <param name="unitOfWork">The unit of work tested.</param>
        void Verify(IUnitOfWork unitOfWork);
    }
}