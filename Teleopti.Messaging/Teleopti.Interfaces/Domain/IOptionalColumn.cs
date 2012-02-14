using System;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for holding optional column.
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 1/13/2009
    /// </remarks>
    public interface IOptionalColumn : IAggregateRoot,
                                        IChangeInfo
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/13/2009
        /// </remarks>
       string Name { get; set; }

       /// <summary>
       /// Gets or sets the name of the table.
       /// </summary>
       /// <value>The name of the table.</value>
       /// <remarks>
       /// Created by: Dinesh Ranasinghe
       /// Created date: 1/13/2009
       /// </remarks>
       string TableName { get; set; }

        /// <summary>
        /// Gets the column value collection.
        /// </summary>
        /// <value>The column value collection.</value>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-07-24
        /// </remarks>
        ReadOnlyCollection<IOptionalColumnValue> ValueCollection { get; }

        /// <summary>
        /// Adds the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-07-24
        /// </remarks>
        void AddOptionalColumnValue(IOptionalColumnValue value);

        /// <summary>
        /// Removes the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-07-24
        /// </remarks>
        void RemoveOptionalColumnValue(IOptionalColumnValue value);

        /// <summary>
        /// Get Optional Column Value by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IOptionalColumnValue GetColumnValueById(Guid? id);
    }
}
