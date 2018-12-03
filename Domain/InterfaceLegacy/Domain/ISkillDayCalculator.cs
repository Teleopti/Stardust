using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Calculator for skill days (helps to calculate skill staff periods demand over several days)
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-09-18
    /// </remarks>
    public interface ISkillDayCalculator
    {
        /// <summary>
        /// Gets the skill.
        /// </summary>
        /// <value>The skill.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-18
        /// </remarks>
        ISkill Skill { get; }

        /// <summary>
        /// Gets the skill days.
        /// </summary>
        /// <value>The skill days.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-18
        /// </remarks>
        IEnumerable<ISkillDay> SkillDays { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is calculated within day.
        /// Only valid after calculation of task periods.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is calculated within day; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-02
        /// </remarks>
        bool IsCalculatedWithinDay { get; }

        /// <summary>
        /// Gets or sets the visible period.
        /// </summary>
        /// <value>The visible period.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-18
        /// </remarks>
        DateOnlyPeriod VisiblePeriod { get; set; }

        /// <summary>
        /// Gets the visible skill days.
        /// </summary>
        /// <value>The visible skill days.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-18
        /// </remarks>
        IEnumerable<ISkillDay> VisibleSkillDays { get; }

        /// <summary>
        /// Gets the skill staff periods.
        /// </summary>
        /// <value>The skill staff periods.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-18
        /// </remarks>
        int SkillStaffPeriodCount { get; }

        /// <summary>
        /// Calculates the task periods.
        /// </summary>
        /// <param name="skillDay">The skill day.</param>
        /// <param name="enableSpillover">if set to <c>true</c> [enable spillover].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-18
        /// </remarks>
        IEnumerable<ITemplateTaskPeriod> CalculateTaskPeriods(ISkillDay skillDay, bool enableSpillover);

        /// <summary>
        /// Finds the next open day.
        /// </summary>
        /// <param name="workload">The workload.</param>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-18
        /// </remarks>
        ISkillDay FindNextOpenDay(IWorkload workload, DateOnly dateTime);

        /// <summary>
        /// Checks the restrictions.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-18
        /// </remarks>
        void CheckRestrictions();

        /// <summary>
        /// Gets the skill staff periods for day calculation.
        /// </summary>
        /// <param name="skillDay">The skill day.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-18
        /// </remarks>
        IEnumerable<ISkillStaffPeriod> GetSkillStaffPeriodsForDayCalculation(ISkillDay skillDay);

        /// <summary>
        /// Clears the skill staff periods.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-18
        /// </remarks>
        void ClearSkillStaffPeriods();

        /// <summary>
        /// Distributes the staff.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-18
        /// </remarks>
        void DistributeStaff();

        /// <summary>
        /// Gets the percentage for interval. Primarily used by multisite skills.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-22
        /// </remarks>
        Percent GetPercentageForInterval(ISkill skill, DateTimePeriod period);

        /// <summary>
        /// Clones to scenario.
        /// </summary>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-12-10
        /// </remarks>
        ISkillDayCalculator CloneToScenario(IScenario scenario);

    	///<summary>
    	/// Invoke unsaved dates.
    	///</summary>
    	///<param name="unsavedDays"></param>
        void InvokeDatesUnsaved(IUnsavedDaysInfo unsavedDays);

        ///<summary>
        /// Finds the next open skill day
        ///</summary>
        ///<param name="skillDay"></param>
        ///<returns></returns>
        ISkillDay FindNextOpenDay(ISkillDay skillDay);
    }
}