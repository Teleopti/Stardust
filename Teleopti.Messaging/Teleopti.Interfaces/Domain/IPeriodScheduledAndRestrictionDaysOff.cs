namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Calculates the number of days off. scheduled and from restrictions
    /// </summary>
    public interface IPeriodScheduledAndRestrictionDaysOff
    {
        /// <summary>
        /// Calculateds the days off.
        /// </summary>
        /// <param name="extractor">The extractor.</param>
        /// <param name="matrix">The matrix.</param>
        /// <param name="useSchedules">if set to <c>true</c> [use schedules].</param>
        /// <param name="usePreferences">if set to <c>true</c> [use preferences].</param>
        /// <param name="useRotations">if set to <c>true</c> [use rotations].</param>
        /// <returns></returns>
        int CalculatedDaysOff(IRestrictionExtractor extractor, IScheduleMatrixPro matrix, bool useSchedules, bool usePreferences, bool useRotations);
    }
}