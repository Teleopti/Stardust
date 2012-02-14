using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    /// <summary>
    /// Sorts by description
    /// </summary>
    public class AbsenceSorter : IComparer<IAbsence>
    {
        /// <summary>
        /// Compare
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(IAbsence x, IAbsence y)
        {
            return string.Compare(x.Description.Name, y.Description.Name, StringComparison.Ordinal);
        }
    }
}
