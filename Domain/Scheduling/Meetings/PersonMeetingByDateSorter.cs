using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
            var xUpdatedOn = x.BelongsToMeeting.UpdatedOn;
            var yUpdatedOn = y.BelongsToMeeting.UpdatedOn;
            if (xUpdatedOn.HasValue && yUpdatedOn.HasValue)
                return xUpdatedOn.Value.CompareTo(yUpdatedOn.Value);
            return x.Period.StartDateTime.CompareTo(y.Period.StartDateTime);
        }
    }
}