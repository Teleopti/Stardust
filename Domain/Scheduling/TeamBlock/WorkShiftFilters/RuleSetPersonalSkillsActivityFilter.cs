using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IRuleSetPersonalSkillsActivityFilter
	{
		IEnumerable<IWorkShiftRuleSet> Filter(IEnumerable<IWorkShiftRuleSet> ruleSetList, IPersonPeriod person, DateOnly dateOnly);

		IEnumerable<IWorkShiftRuleSet> FilterForRoleModel(IEnumerable<IWorkShiftRuleSet> ruleSetList, ITeamInfo team,
			DateOnly dateOnly);
	}

	public class RuleSetPersonalSkillsActivityFilter : IRuleSetPersonalSkillsActivityFilter
	{
		private readonly IRuleSetSkillActivityChecker _ruleSetSkillActivityChecker;
		private readonly PersonalSkillsProvider _personalSkillsProvider;

		public RuleSetPersonalSkillsActivityFilter(IRuleSetSkillActivityChecker ruleSetSkillActivityChecker, PersonalSkillsProvider personalSkillsProvider)
		{
			_ruleSetSkillActivityChecker = ruleSetSkillActivityChecker;
			_personalSkillsProvider = personalSkillsProvider;
		}

		public IEnumerable<IWorkShiftRuleSet> Filter(IEnumerable<IWorkShiftRuleSet> ruleSetList, IPersonPeriod person, DateOnly dateOnly)
		{
			var personalSkills = _personalSkillsProvider.PersonSkillsBasedOnPrimarySkill(person);
			var skills = personalSkills.Where(
					personalSkill => personalSkill.Active && !((IDeleteTag) personalSkill.Skill).IsDeleted)
				.Select(personalSkill => personalSkill.Skill).ToArray();
			return
				ruleSetList.Where(workShiftRuleSet => _ruleSetSkillActivityChecker.CheckSkillActivities(workShiftRuleSet, skills));
		}

		public IEnumerable<IWorkShiftRuleSet> FilterForRoleModel(IEnumerable<IWorkShiftRuleSet> ruleSetList, ITeamInfo team, DateOnly dateOnly)
		{
			var groupMembers = team.GroupMembers.Select(x => x.Period(dateOnly)).Where(x => x != null);
			var workShiftRuleSets = ruleSetList as IList<IWorkShiftRuleSet> ?? ruleSetList.ToList();
			var memberList = groupMembers as IList<IPersonPeriod> ?? groupMembers.ToList();
			var commonList = Filter(workShiftRuleSets, memberList.First(), dateOnly).ToList();

			foreach (var groupMember in memberList)
			{
				var memberRuleSets = Filter(workShiftRuleSets, groupMember, dateOnly).ToList();
				var toRemove = commonList.Except(memberRuleSets).ToArray();
				foreach (var workShiftRuleSet in toRemove)
				{
					commonList.Remove(workShiftRuleSet);
				}
			}

			return commonList;
		}
	}
}