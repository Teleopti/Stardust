namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Calculates the daily value
    /// </summary>
    public interface IScheduleResultDailyValueCalculator 
        : IScheduleResultDataExtractor
    {
        /// <summary>
        /// Calculates the day value.
        /// </summary>
        /// <param name="scheduleDay">The schedule day.</param>
        /// <returns></returns>
        double? DayValue(DateOnly scheduleDay);
    }
}