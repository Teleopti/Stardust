using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IRuleSetPersonalSkillsActivityFilter
	{
		IEnumerable<IWorkShiftRuleSet> Filter(IEnumerable<IWorkShiftRuleSet> ruleSetList, IPerson person, DateOnly dateOnly);

		IEnumerable<IWorkShiftRuleSet> FilterForRoleModel(IEnumerable<IWorkShiftRuleSet> ruleSetList, ITeamInfo team,
			DateOnly dateOnly);
	}

	public class RuleSetPersonalSkillsActivityFilter : IRuleSetPersonalSkillsActivityFilter
	{
		private readonly IRuleSetSkillActivityChecker _ruleSetSkillActivityChecker;

		public RuleSetPersonalSkillsActivityFilter(IRuleSetSkillActivityChecker ruleSetSkillActivityChecker)
		{
			_ruleSetSkillActivityChecker = ruleSetSkillActivityChecker;
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

		public IEnumerable<IWorkShiftRuleSet> FilterForRoleModel(IEnumerable<IWorkShiftRuleSet> ruleSetList, ITeamInfo team, DateOnly dateOnly)
		{
			//var groupMembers = team.GroupMembers.Intersect(team.UnLockedMembers(dateOnly));
			var groupMembers = team.GroupMembers.Where(x => x.Period(dateOnly) != null);
			var workShiftRuleSets = ruleSetList as IList<IWorkShiftRuleSet> ?? ruleSetList.ToList();
			var memberList = groupMembers as IList<IPerson> ?? groupMembers.ToList();
			var commonList = Filter(workShiftRuleSets, memberList.First(), dateOnly).ToList();

			foreach (var groupMember in memberList)
			{
				var memberRuleSets = Filter(workShiftRuleSets, groupMember, dateOnly).ToList();
				var toRemove = new List<IWorkShiftRuleSet>();
				foreach (var workShiftRuleSet in commonList)
				{
					if(!memberRuleSets.Contains(workShiftRuleSet))
						toRemove.Add(workShiftRuleSet);
				}
				foreach (var workShiftRuleSet in toRemove)
				{
					commonList.Remove(workShiftRuleSet);
				}
			}

			return commonList;
		}
	}
}