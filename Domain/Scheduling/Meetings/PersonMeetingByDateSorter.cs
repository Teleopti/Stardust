using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
    /// <summary>
    /// Sorter PersonMeeting by date
    /// </summary>
    public class PersonMeetingByDateSorter : IComparer<IPersonMeeting>
    {
        /// <summary>
        /// Comparer for PersonMeeting by date
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(IPersonMeeting x, IPersonMeeting y)
        {
            return x.Period.StartDateTime.CompareTo(y.Period.StartDateTime);
        }
    }
}