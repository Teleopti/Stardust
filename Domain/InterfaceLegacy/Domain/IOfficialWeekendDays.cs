using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Stores and retrieves the official week-start and WeekEnd days for a given culture.
    /// </summary>
    public interface IOfficialWeekendDays
    {
        /// <summary>
        /// Gets the weekend day indexes.
        /// </summary>
        /// <returns></returns>
        IList<int> WeekendDayIndexesRelativeStartDayOfWeek();
    }
}