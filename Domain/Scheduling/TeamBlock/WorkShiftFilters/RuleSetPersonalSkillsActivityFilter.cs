

using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IRuleSetPersonalSkillsActivityFilter
	{
		IEnumerable<IWorkShiftRuleSet> Filter(IEnumerable<IWorkShiftRuleSet> ruleSetList, IPerson person, DateOnly dateOnly);
	}

	public class RuleSetPersonalSkillsActivityFilter : IRuleSetPersonalSkillsActivityFilter
	{
		private readonly IRuleSetSkillActivityChecker _ruleSetSkillActivityChecker;

		public RuleSetPersonalSkillsActivityFilter(IRuleSetSkillActivityChecker _ruleSetSkillActivityChecker)
		{
			this._ruleSetSkillActivityChecker = _ruleSetSkillActivityChecker;
		}

		public IEnumerable<IWorkShiftRuleSet> Filter(IEnumerable<IWorkShiftRuleSet> ruleSetList, IPerson person, DateOnly dateOnly)
		{
			var personalSkills = person.Period(dateOnly).PersonSkillCollection;
			var skills = new List<ISkill>();
			foreach (var personalSkill in personalSkills)
			{
				if (personalSkill.Active && !((IDeleteTag)personalSkill.Skill).IsDeleted)
					skills.Add(personalSkill.Skill);
			}
			var filteredRulesets = new List<IWorkShiftRuleSet>();
			foreach (var workShiftRuleSet in ruleSetList)
			{
				if (_ruleSetSkillActivityChecker.CheckSkillActivities(workShiftRuleSet, skills))
					filteredRulesets.Add(workShiftRuleSet);

			}
			return filteredRulesets;
		}
	}
}