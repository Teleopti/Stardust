namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Extracted from old IBusinessRule. Don't really know what it is but...
    /// Something about knowing length of poss assignmnents bla bla
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2010-04-22
    /// </remarks>
    public interface IAssignmentPeriodRule
    {
        
        /// <summary>
        /// Longests the date time period for assignment.
        /// </summary>
        /// <param name="current">The current.</param>
        /// <param name="dateToCheck">The date to check.</param>
        /// <returns></returns>
        DateTimePeriod LongestDateTimePeriodForAssignment(IScheduleRange current, DateOnly dateToCheck);
    }
}