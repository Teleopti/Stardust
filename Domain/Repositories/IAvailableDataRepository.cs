using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

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

        /// <summary>
        /// Loads all collections in available data.
        /// </summary>
        /// <param name="availableData">The available data.</param>
        /// <returns></returns>
        IAvailableData LoadAllCollectionsInAvailableData(IAvailableData availableData);
    }
}
