using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    /// <summary>
    /// Sorts persondayoffs by its anchor date.
    /// Tested from scheduletest.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-05-26
    /// </remarks>
    public class PersonDayOffByDateSorter : IComparer<IPersonDayOff>
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
        /// Created date: 2008-05-26
        /// </remarks>
        public int Compare(IPersonDayOff x, IPersonDayOff y)
        {
            return x.Period.StartDateTime.CompareTo(y.Period.StartDateTime);
        }
    }
}
