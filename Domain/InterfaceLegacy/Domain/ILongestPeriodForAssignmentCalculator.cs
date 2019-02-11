namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Blabla - dont know really. Copy/paste from old businessrules
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2010-04-22
    /// </remarks>
    public interface ILongestPeriodForAssignmentCalculator
    {
        /// <summary>
        /// Possibles the period.
        /// </summary>
        /// <param name="current">The current.</param>
        /// <param name="dateToCheck">The date to check.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2010-04-22
        /// </remarks>
        DateTimePeriod? PossiblePeriod(IScheduleRange current, DateOnly dateToCheck, bool scheduleOnDayOffs);
    }
}