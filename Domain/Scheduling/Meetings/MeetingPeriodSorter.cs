using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Meetings;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
    /// <summary>
    /// Sorter for MeetingPeriod
    /// </summary>
    public class MeetingPeriodSorter : IComparer<MeetingPeriod>
    {
        /// <summary>
        /// Sort descending on value
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(MeetingPeriod x, MeetingPeriod y)
        {
            return (y.Value.CompareTo(x.Value));
        }
    }
}