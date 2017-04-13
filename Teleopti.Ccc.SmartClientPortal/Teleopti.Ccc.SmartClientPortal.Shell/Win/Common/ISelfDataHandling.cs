using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common
{
    /// <summary>
    /// Interface for general datahandling on a form.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 12/10/2007
    /// </remarks>
    public interface ISelfDataHandling
    {
        /// <summary>
        /// Sets the unit of work.
        /// Typically calls for the ChangeObjectsUnitOfWork method in base class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-03
        /// </remarks>
        void SetUnitOfWork(IUnitOfWork value);

        /// <summary>
        /// Persists the data instance.
        /// </summary>
        void Persist();

    }
}
