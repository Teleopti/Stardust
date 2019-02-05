using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Skill day
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-09-18
    /// </remarks>
    public interface ISkillDay : ITaskOwner, IRestrictionChecker<ISkillDay>, ITemplateDay, ICloneableEntity<ISkillDay>, IAggregateRoot, IChangeInfo, IFilterOnBusinessUnit
    {
        /// <summary>
        /// Gets the Scenario
        /// </summary>
        IScenario Scenario { get; }

        /// <summary>
        /// Gets the skill.
        /// </summary>
        /// <value>The skill.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 18.12.2007
        /// </remarks>
        ISkill Skill { get; }

        /// <summary>
        /// Gets the skill data period collection.
        /// </summary>
        /// <value>The skill data period collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-30
        /// </remarks>
        ReadOnlyCollection<ISkillDataPeriod> SkillDataPeriodCollection { get; }

        /// <summary>
        /// Gets the workload day collection.
        /// </summary>
        /// <value>The workload day collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        ReadOnlyCollection<IWorkloadDay> WorkloadDayCollection { get; }

        /// <summary>
        /// Gets the forecasted incoming demand for the skill day.
        /// </summary>
        /// <value>The forecasted incoming demand.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-08
        /// </remarks>
        TimeSpan ForecastedIncomingDemand { get; }

        /// <summary>
        /// Gets the forecasted hours with shrinkage.
        /// </summary>
        /// <value>The forecasted hours with shrinkage.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-08
        /// </remarks>
        TimeSpan ForecastedIncomingDemandWithShrinkage { get; }

        /// <summary>
        /// Gets the forecasted distributed demand.
        /// </summary>
        /// <value>The forecasted distributed demand.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-05
        /// </remarks>
        TimeSpan ForecastedDistributedDemand { get; }

        /// <summary>
        /// Gets the forecasted distributed demand with shrinkage.
        /// </summary>
        /// <value>The forecasted distributed demand with shrinkage.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-05
        /// </remarks>
        TimeSpan ForecastedDistributedDemandWithShrinkage { get; }

        /// <summary>
        /// Gets the calculated staff collection.
        /// </summary>
        /// <value>The calculated staff collection.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-22
        /// </remarks>
        ISkillStaffPeriod[] SkillStaffPeriodCollection { get; }

        /// <summary>
        /// Gets or sets the skill day calculator.
        /// </summary>
        /// <value>The skill day calculator.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-02
        /// </remarks>
        ISkillDayCalculator SkillDayCalculator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable spill over].
        /// </summary>
        /// <value><c>true</c> if [enable spill over]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-04
        /// </remarks>
        bool EnableSpillover { get; set; }

        /// <summary>
        /// Gets the complete skill staff period collection.
        /// </summary>
        /// <value>The complete skill staff period collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-08
        /// </remarks>
        ISkillStaffPeriod[] CompleteSkillStaffPeriodCollection { get; }

        /// <summary>
        /// Adds the workload day.
        /// Temporary function to add a workload day to the skill day collection
        /// </summary>
        /// <param name="workloadDay">The workload day.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        void AddWorkloadDay(IWorkloadDay workloadDay);

        /// <summary>
        /// Applies the template on a skillday.
        /// </summary>
        /// <param name="skillDayTemplate">The skill day template.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-04
        /// </remarks>
        void ApplyTemplate(ISkillDayTemplate skillDayTemplate);

        /// <summary>
        /// Creates from template.
        /// </summary>
        /// <param name="skillDayDate">The skill day date.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="skillDayTemplate">The skill day template.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-15
        /// </remarks>
        void CreateFromTemplate(DateOnly skillDayDate, ISkill skill, IScenario scenario, ISkillDayTemplate skillDayTemplate);

        /// <summary>
        /// Merges the skill data periods.
        /// Only skill data periods owned by this skill day will be merged!
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        void MergeSkillDataPeriods(IList<ISkillDataPeriod> list);

        /// <summary>
        /// Occurs when [staff recalculated].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-06
        /// </remarks>
        event EventHandler<EventArgs> StaffRecalculated;
		
        /// <summary>
        /// Recalculate staff. Use when periods/openhours have not been changed
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-25
        /// </remarks>
        void RecalculateStaff();

        /// <summary>
        /// Splits the skill data periods.
        /// Only skill data periods owned by this skill day will be splitted!
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        void SplitSkillDataPeriods(IList<ISkillDataPeriod> list);

		/// <summary>
		/// Returns a ReadOnlyCollection with a projection of all  openhours for this skillday
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: henrika
		/// Created date: 2008-04-23
		/// </remarks>
		IEnumerable<TimePeriod> OpenHours();

        /// <summary>
        /// Sets the calculated staff collection.
        /// </summary>
        /// <param name="newSkillStaffPeriodValues">The updated staff periods.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-07
        /// </remarks>
        void SetCalculatedStaffCollection(INewSkillStaffPeriodValues newSkillStaffPeriodValues);

        /// <summary>
        /// Setups the skill day.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-18
        /// </remarks>
        void SetupSkillDay();

        /// <summary>
        /// Clones the Entity to another Scenario
        /// </summary>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-12-10
        /// </remarks>
        ISkillDay NoneEntityClone(IScenario scenario);

        /// <summary>
        /// Sets the new skill data period collection.
        /// </summary>
        /// <param name="newSkillDataPeriods">The new skill data periods.</param>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2009-01-26
        /// </remarks>
        void SetNewSkillDataPeriodCollection(IList<ISkillDataPeriod> newSkillDataPeriods);

        /// <summary>
        /// Resets the skill staff periods.
        /// </summary>
        /// <param name="skillDataPeriod">The skill data period.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-05-13
        /// </remarks>
        void ResetSkillStaffPeriods(ISkillDataPeriod skillDataPeriod);

	    /// <summary>
	    /// Splits SkillStaffPeriodCollection into a new collection of ISkillStaffPeriodViews.
	    /// The specified period length must be shorter than on this and the split must be even.
	    /// For example can not a collection of periods in 15 minutes period be split on 10. But 5.
	    /// </summary>
	    /// <param name="periodLength">Length of the period in the new collection.</param>
	    /// <param name="useShrinkage"></param>
	    /// <returns></returns>
	    /// /// 
	    /// <remarks>
	    ///  Created by: Ola
	    ///  Created date: 2009-07-06    
	    /// /// </remarks>
	    ISkillStaffPeriodView[] SkillStaffPeriodViewCollection(TimeSpan periodLength, bool useShrinkage = false);

	    void OpenAllSkillStaffPeriods(int maxSeats);
    }

	public static class SkillDayExtensions
	{
		public static int IntervalLengthInMinutes(this IEnumerable<ISkillDay> skillDays)
		{
			return skillDays.Any() ? skillDays.Min(s => s.Skill.DefaultResolution) : 15;
		}
	}
}