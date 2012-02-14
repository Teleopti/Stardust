using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Service providing scheduling result
    /// </summary>
    public interface ISchedulingResultService
    {
        /// <summary>
        /// Create or recreate scheduling result for the complete loaded period.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-08-22
        /// </remarks>
        ISkillSkillStaffPeriodExtendedDictionary SchedulingResult();

        /// <summary>
        /// Create or recreate scheduling result for the given period.
        /// </summary>
        /// <param name="periodToRecalculate">The period to recalculate.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-08-22
        /// </remarks>
        ISkillSkillStaffPeriodExtendedDictionary SchedulingResult(DateTimePeriod periodToRecalculate);

        /// <summary>
        /// Create or recreate scheduling result for the given period.
        /// </summary>
        /// <param name="periodToRecalculate">The period to recalculate.</param>
        /// <param name="emptyCache">if set to <c>true</c> [empty cache].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2008-08-22
        /// if called from domain this call seems to work.
        /// </remarks>
        ISkillSkillStaffPeriodExtendedDictionary SchedulingResult(DateTimePeriod periodToRecalculate, bool emptyCache);

    }
}