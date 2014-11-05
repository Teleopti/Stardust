using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface IIntraIntervalIssueCalculator
	{
		IIntraIntervalIssues CalculateIssues(ISchedulingResultStateHolder schedulingResultStateHolder, ISkill skill, DateOnly dateOnly);
	}

	public class IntraIntervalIssueCalculator : IIntraIntervalIssueCalculator
	{
		private readonly ISkillDayIntraIntervalIssueExtractor _skillDayIntraIntervalIssueExtractor;

		public IntraIntervalIssueCalculator(ISkillDayIntraIntervalIssueExtractor skillDayIntraIntervalIssueExtractor)
		{
			_skillDayIntraIntervalIssueExtractor = skillDayIntraIntervalIssueExtractor;
		}

		public IIntraIntervalIssues CalculateIssues(ISchedulingResultStateHolder schedulingResultStateHolder, ISkill skill, DateOnly dateOnly)
		{
			var result = new IntraIntervalIssues();
			//var skillDaysOnDayBefore = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { dateOnly.AddDays(-1) });
			var skillDaysOnDay = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { dateOnly });
			var skillDaysOnDayAfter = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { dateOnly.AddDays(1) });

			//result.IssuesOnDayBefore = _skillDayIntraIntervalIssueExtractor.Extract(skillDaysOnDayBefore, skill);
			result.IssuesOnDay = _skillDayIntraIntervalIssueExtractor.Extract(skillDaysOnDay, skill);
			result.IssuesOnDayAfter = _skillDayIntraIntervalIssueExtractor.Extract(skillDaysOnDayAfter, skill);

			return result;
		}		
	}
}
