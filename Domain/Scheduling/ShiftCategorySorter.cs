using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    /// <summary>
    /// Sorts by description
    /// </summary>
    public class ShiftCategorySorter : IComparer<IShiftCategory>
    {
        /// <summary>
        /// Compare
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(IShiftCategory x, IShiftCategory y)
        {
            return string.Compare(x.Description.Name, y.Description.Name, StringComparison.Ordinal);
        }
    }
}
