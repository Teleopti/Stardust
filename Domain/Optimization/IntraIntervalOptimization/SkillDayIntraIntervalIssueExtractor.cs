using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface ISkillDayIntraIntervalIssueExtractor
	{
		IList<ISkillStaffPeriod> ExtractOnIssues(IEnumerable<ISkillDay> skillDays, ISkill skill);
		IList<ISkillStaffPeriod> ExtractAll(IEnumerable<ISkillDay> skillDays, ISkill skill);
	}

	public class SkillDayIntraIntervalIssueExtractor : ISkillDayIntraIntervalIssueExtractor
	{
		public IList<ISkillStaffPeriod> ExtractOnIssues(IEnumerable<ISkillDay> skillDays, ISkill skill)
		{
			var result = new List<ISkillStaffPeriod>();

			foreach (var skillDay in skillDays)
			{
				if (skillDay.Skill != skill) continue;

				result.AddRange(skillDay.SkillStaffPeriodCollection.Where(s => s.HasIntraIntervalIssue).Select(s => (ISkillStaffPeriod)s.NoneEntityClone()));
			}

			return result;	
		}

		public IList<ISkillStaffPeriod> ExtractAll(IEnumerable<ISkillDay> skillDays, ISkill skill)
		{
			var result = new List<ISkillStaffPeriod>();

			foreach (var skillDay in skillDays)
			{
				if (skillDay.Skill != skill) continue;

				result.AddRange(skillDay.SkillStaffPeriodCollection.Select(skillStaffPeriod =>
					(ISkillStaffPeriod) skillStaffPeriod.NoneEntityClone()));
			}

			return result;
		}
	}
}
