using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Stores and retrieves the official week-start and WeekEnd days for a given culture.
    /// </summary>
    public interface IOfficialWeekendDays
    {
        /// <summary>
        /// Gets the week start day.
        /// </summary>
        /// <value>The week start day.</value>
        DayOfWeek WeekStartDay { get; }

        /// <summary>
        /// Gets the official weekends the days.
        /// </summary>
        /// <returns></returns>
        IList<DayOfWeek> WeekendDays();

        /// <summary>
        /// Gets the weekend day indexes.
        /// </summary>
        /// <returns></returns>
        IList<int> WeekendDayIndexes();
    }
}