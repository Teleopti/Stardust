using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
        IList<IOptionalColumn> GetOptionalColumns<T>();

		IList<IColumnUniqueValues> UniqueValuesOnColumn(Guid column);
		List<IOptionalColumnValue> OptionalColumnValues(Guid optionalColumnId);
    }

	public interface IColumnUniqueValues
	{
		string Description { get; set; }
	}
}
