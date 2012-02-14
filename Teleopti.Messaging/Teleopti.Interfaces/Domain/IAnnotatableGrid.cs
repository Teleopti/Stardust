using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// If you wish to use comments in cells in your grid, you are going to need to implement this in your grid.
    /// </summary>
    /// <remarks>
    /// Created by: HenryG
    /// Created date: 2008-10-10
    /// </remarks>
    public interface IAnnotatableGrid
    {
        /// <summary>
        /// Gets the annotatable object.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-10-14
        /// </remarks>
        IAnnotatable GetAnnotatableObject(int index);

        /// <summary>
        /// Determines whether [is annotatable cell] [the specified column index].
        /// </summary>
        /// <param name="columnIndex">Index of the column.</param>
        /// <param name="rowIndex">Index of the row.</param>
        /// <returns>
        /// 	<c>true</c> if [is annotatable cell] [the specified column index]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-10-14
        /// </remarks>
        bool IsAnnotatableCell(int columnIndex, int rowIndex);


    }
}
