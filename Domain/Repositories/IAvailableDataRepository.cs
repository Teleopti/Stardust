using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    ///  Interface for AvailableDataRepository.
    /// </summary>
    public interface IAvailableDataRepository : IRepository<IAvailableData>
    {
        /// <summary>
        /// Reads all available data
        /// </summary>
        /// <returns>The AvailableData list.</returns>
        IList<IAvailableData> LoadAllAvailableData();
    }
}
