using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public class IntraIntervalIssueCalculator
	{
		private readonly ISkillDayIntraIntervalIssueExtractor _skillDayIntraIntervalIssueExtractor;

		public IntraIntervalIssueCalculator(ISkillDayIntraIntervalIssueExtractor skillDayIntraIntervalIssueExtractor)
		{
			_skillDayIntraIntervalIssueExtractor = skillDayIntraIntervalIssueExtractor;
		}

		public IntraIntervalIssues CalculateIssues(ISchedulingResultStateHolder schedulingResultStateHolder, ISkill skill, DateOnly dateOnly)
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
