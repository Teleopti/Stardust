using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface IIntraIntervalIssues
	{
		IList<ISkillStaffPeriod> IssuesOnDayBefore { get; set; }
		IList<ISkillStaffPeriod> IssuesOnDay { get; set; }
		IList<ISkillStaffPeriod> IssuesOnDayAfter { get; set; }
		void CalculateIssues(ISchedulingResultStateHolder schedulingResultStateHolder, ISkill skill, DateOnly dateOnly);
		void Clear();
		IntraIntervalIssues Clone();
	}

	public class IntraIntervalIssues : IIntraIntervalIssues
	{
		private readonly ISkillDayIntraIntervalIssueExtractor _skillDayIntraIntervalIssueExtractor;

		public IntraIntervalIssues(ISkillDayIntraIntervalIssueExtractor skillDayIntraIntervalIssueExtractor)
		{
			_skillDayIntraIntervalIssueExtractor = skillDayIntraIntervalIssueExtractor;
		}

		public IList<ISkillStaffPeriod> IssuesOnDayBefore { get; set; }
		public IList<ISkillStaffPeriod> IssuesOnDay { get; set; }
		public IList<ISkillStaffPeriod> IssuesOnDayAfter { get; set; }

		public void CalculateIssues(ISchedulingResultStateHolder schedulingResultStateHolder, ISkill skill, DateOnly dateOnly)
		{
			var skillDaysOnDayBefore = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { dateOnly.AddDays(-1) });
			var skillDaysOnDay = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { dateOnly });
			var skillDaysOnDayAfter = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { dateOnly.AddDays(1) });

			IssuesOnDayBefore = _skillDayIntraIntervalIssueExtractor.Extract(skillDaysOnDayBefore, skill);
			IssuesOnDay = _skillDayIntraIntervalIssueExtractor.Extract(skillDaysOnDay, skill);
			IssuesOnDayAfter = _skillDayIntraIntervalIssueExtractor.Extract(skillDaysOnDayAfter, skill);
		}

		public void Clear()
		{
			IssuesOnDayBefore = new List<ISkillStaffPeriod>();
			IssuesOnDay = new List<ISkillStaffPeriod>();
			IssuesOnDayAfter = new List<ISkillStaffPeriod>();
		}

		public IntraIntervalIssues Clone()
		{
			var clone = new IntraIntervalIssues(_skillDayIntraIntervalIssueExtractor);

			IList<ISkillStaffPeriod> before = new List<ISkillStaffPeriod>();
			IList<ISkillStaffPeriod> onDay = new List<ISkillStaffPeriod>();
			IList<ISkillStaffPeriod> after = new List<ISkillStaffPeriod>();

			foreach (var skillStaffPeriod in IssuesOnDayBefore)
			{
				before.Add((ISkillStaffPeriod)skillStaffPeriod.NoneEntityClone());
			}

			foreach (var skillStaffPeriod in IssuesOnDay)
			{
				onDay.Add((ISkillStaffPeriod)skillStaffPeriod.NoneEntityClone());
			}

			foreach (var skillStaffPeriod in IssuesOnDayAfter)
			{
				after.Add((ISkillStaffPeriod)skillStaffPeriod.NoneEntityClone());
			}

			clone.IssuesOnDayBefore = before;
			clone.IssuesOnDay = onDay;
			clone.IssuesOnDayAfter = after;

			return clone;
		}
	}
}