using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    /// <summary>
    /// Sorts by person absences by priority, lastchanged
    /// 
    /// If needed, add stuff like ascending, descending etc.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-02-22
    /// </remarks>
    public class PersonAbsenceSorter : IComparer<IPersonAbsence>
    {
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value Condition Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-02-22
        /// </remarks>
        public int Compare(IPersonAbsence x, IPersonAbsence y)
        {
            int ret = y.Layer.Payload.Priority.CompareTo(x.Layer.Payload.Priority);
            if (ret == 0)
            {
                if (!x.LastChange.HasValue && !y.LastChange.HasValue)
                {
                    return 0;
                }
                if (!x.LastChange.HasValue)
                {
                    return -1;
                }
                if (!y.LastChange.HasValue)
                {
                    return 1;
                }
                ret = x.LastChange.Value.CompareTo(y.LastChange.Value);
            }
            return ret;
        }
    }
}
