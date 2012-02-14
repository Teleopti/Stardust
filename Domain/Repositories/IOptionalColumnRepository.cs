using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{

    /// <summary>
    /// Defines the functionality of a IOptionalColumnRepository
    /// </summary>
    public interface IOptionalColumnRepository : IRepository<IOptionalColumn>
    {
        /// <summary>
        /// Populates the given entity list with the appropriate optional columns and the values and
        /// returns the populated collection.
        /// </summary>
        /// <typeparam name="T">Type of the entity which optional columns
        /// are available.
        /// </typeparam>
        /// <returns></returns>
        IList<IOptionalColumn> GetOptionalColumnValues<T>();
    }
}
