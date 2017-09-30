using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Represents a scheduled week.
    /// </summary>
    public interface IContractScheduleWeek : IAggregateEntity
    {
        /// <summary>
        /// Sort order for Week
        /// </summary>
        int WeekOrder { get; set; }

        /// <summary>
        /// Check if given day is a work day for the week
        /// </summary>
        /// <param name="dayOfWeek">Week day</param>
        /// <returns>True if it's a work day, otherwise False.</returns>
        bool IsWorkday(DayOfWeek dayOfWeek);

        /// <summary>
        /// Get information about given week day
        /// </summary>
        /// <param name="dayOfWeek">Week day</param>
        /// <returns>True if it's a work day, otherwise False.</returns>
        bool this[DayOfWeek dayOfWeek] { get; }

        /// <summary>
        /// Add new definition to collection (or replace old)
        /// </summary>
        /// <param name="dayOfWeek">Week day</param>
        /// <param name="isWorkday">True if day is work day</param>
        void Add(DayOfWeek dayOfWeek, bool isWorkday);
    }
}
