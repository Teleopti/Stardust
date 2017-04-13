namespace Teleopti.Ccc.Win.Common.Controls
{
    /// <summary>
    /// Determines which type of date header to show
    /// </summary>
    public enum DateHeaderType
    {
        /// <summary>
        /// Short date
        /// </summary>
        Date,
        /// <summary>
        /// Week day name
        /// </summary>
        WeekdayName,
        /// <summary>
        /// Short dates for week
        /// </summary>
        WeekDates,
        /// <summary>
        /// Week number
        /// </summary>
        WeekNumber,
        /// <summary>
        /// Name of month
        /// </summary>
        MonthName,
        /// <summary>
        /// Month name and year
        /// </summary>
        MonthNameYear,
        /// <summary>
        /// Year
        /// </summary>
        Year,

        /// <summary>
        /// the datenumber for the day in month: ie 21 june -> 21
        /// </summary>
        MonthDayNumber,
		/// <summary>
		/// Period
		/// </summary>
		Period

    }
}
