using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Weekly recurrence for meetings
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2009-10-13
    /// </remarks>
    public interface IRecurrentWeeklyMeeting : IRecurrentMeetingOption
    {
        /// <summary>
        /// Gets the week days.
        /// </summary>
        /// <value>The week days.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-13
        /// </remarks>
		IEnumerable<DayOfWeek> WeekDays { get; }

        /// <summary>
        /// Clears the week days.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-13
        /// </remarks>
        void ClearWeekDays();

        /// <summary>
        /// Gets or sets the <see cref="System.Boolean"/> with the specified day of week.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-13
        /// </remarks>
        bool this[DayOfWeek dayOfWeek] { get; set; }
    }
}