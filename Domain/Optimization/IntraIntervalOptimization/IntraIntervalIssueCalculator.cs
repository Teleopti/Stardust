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
			
			var skillDaysOnDay = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { dateOnly });
			var skillDaysOnDayAfter = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { dateOnly.AddDays(1) });

			result.IssuesOnDay = _skillDayIntraIntervalIssueExtractor.ExtractOnIssues(skillDaysOnDay, skill);
			result.IssuesOnDayAfter = _skillDayIntraIntervalIssueExtractor.ExtractOnIssues(skillDaysOnDayAfter, skill);

			return result;
		}		
	}
}
