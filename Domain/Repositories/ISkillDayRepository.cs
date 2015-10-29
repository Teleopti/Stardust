using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// Interface for mocking only
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2008-02-25
    /// </remarks>
	public interface ISkillDayRepository : IRepository<ISkillDay>
    {
        /// <summary>
        /// Finds the range.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        ICollection<ISkillDay> FindRange(DateOnlyPeriod period, ISkill skill, IScenario scenario);

        /// <summary>
        /// Gets all skill days.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="skillDays">The skill days.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="addToRepository">if set to <c>true</c> [add to repository].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        ICollection<ISkillDay> GetAllSkillDays(DateOnlyPeriod period, ICollection<ISkillDay> skillDays, ISkill skill, IScenario scenario, Action<IEnumerable<ISkillDay>> optionalAction);

        /// <summary>
        /// Deletes the specified date time period.
        /// </summary>
        /// <param name="dateTimePeriod">The date time period.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-12-11
        /// </remarks>
        void Delete(DateOnlyPeriod dateTimePeriod, ISkill skill, IScenario scenario);

        /// <summary>
        /// Finds the last skill day date.
        /// </summary>
        /// <param name="workload">The workload.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-04-08
        /// </remarks>
        DateOnly FindLastSkillDayDate(IWorkload workload, IScenario scenario);

        ISkillDay FindLatestUpdated(ISkill skill, IScenario scenario, bool withLongterm);

        ICollection<ISkillDay> FindRange(DateOnlyPeriod period, IList<ISkill> skills, IScenario scenario);

	    IEnumerable<SkillTaskDetailsModel> GetSkillsTasksDetails(DateTimePeriod period, IList<ISkill> skills, IScenario scenario);
    }

	//refactor this PBI 35176
	public class SkillTaskDetailsModel
	{
		public string Name { get; set; }
		public DateTime Minimum { get; set; }
		public DateTime Maximum { get; set; }
		public Double TotalTasks { get; set; }
		public Guid Scenario { get; set; }
		public Guid SkillId { get; set; }
	}
}