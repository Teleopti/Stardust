using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Calculates the forecast and scheduled value for a skill for the given period.
    /// </summary>
    public interface IDailySkillForecastAndScheduledValueCalculator
    {
        /// <summary>
        /// Calculates the daily forecast and schedule data for a skill and a period.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <param name="scheduleDay">The schedule day.</param>
        /// <returns></returns>
        ForecastScheduleValuePair CalculateDailyForecastAndScheduleDataForSkill(ISkill skill, DateOnly scheduleDay);

        /// <summary>
        /// Calculates the skill staff period.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <param name="skillStaffPeriod">The skill staff period.</param>
        /// <returns></returns>
        double CalculateSkillStaffPeriod(ISkill skill, ISkillStaffPeriod skillStaffPeriod);
    }
}