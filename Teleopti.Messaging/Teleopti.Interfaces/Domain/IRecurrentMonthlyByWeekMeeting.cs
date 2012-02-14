using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Recurring information for meetings a certain week day and number of week in month
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2009-10-23
    /// </remarks>
    public interface IRecurrentMonthlyByWeekMeeting : IRecurrentMeetingOption
    {
        /// <summary>
        /// Gets or sets the week of month.
        /// </summary>
        /// <value>The week of month.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-23
        /// </remarks>
        WeekNumber WeekOfMonth { get; set; }

        /// <summary>
        /// Gets or sets the day of week.
        /// </summary>
        /// <value>The day of week.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-23
        /// </remarks>
        DayOfWeek DayOfWeek { get; set; }
    }
}