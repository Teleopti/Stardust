using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public abstract class FillSchedulerStateHolder
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;

		protected FillSchedulerStateHolder(PersonalSkillsProvider personalSkillsProvider)
		{
			_personalSkillsProvider = personalSkillsProvider;
		}

		[TestLog]
		public virtual void Fill(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentsInIsland, 
			IEnumerable<Guid> choosenAgents, LockInfoForStateHolder lockInfoForStateHolder, 
			DateOnlyPeriod period, IEnumerable<Guid> onlyUseSkills = null)
		{
			PreFill(schedulerStateHolderTo, period);
			var scenario = FetchScenario();
			schedulerStateHolderTo.SetRequestedScenario(scenario);
			FillAgents(schedulerStateHolderTo, agentsInIsland, choosenAgents, period);
			removeUnwantedAgents(schedulerStateHolderTo, agentsInIsland);
			var skills = skillsToUse(schedulerStateHolderTo.SchedulingResultState.LoadedAgents, period, onlyUseSkills);
			skills = includeParentMultisiteSkills(skills);
			FillSkillDays(schedulerStateHolderTo, scenario, skills, period);
			removeUnwantedSkillsAndSkillDays(schedulerStateHolderTo, skills);
			FillSchedules(schedulerStateHolderTo, scenario, schedulerStateHolderTo.SchedulingResultState.LoadedAgents, period);
			removeUnwantedScheduleRanges(schedulerStateHolderTo);
			PostFill(schedulerStateHolderTo, period);
			setLocks(schedulerStateHolderTo, lockInfoForStateHolder);
			schedulerStateHolderTo.ResetFilteredPersons();
		}

		private IEnumerable<ISkill> includeParentMultisiteSkills(IEnumerable<ISkill> skills)
		{
			var childSkills = skills.OfType<IChildSkill>();
			var multisiteSkills = childSkills.Select(c => c.ParentSkill).Distinct();
			return skills.Concat(multisiteSkills);
		}

		private static void setLocks(ISchedulerStateHolder schedulerStateHolderTo, LockInfoForStateHolder lockInfoForStateHolder)
		{
			if (lockInfoForStateHolder == null)
				return;

			foreach (var lockInfo in lockInfoForStateHolder.Locks)
			{
				var agent = schedulerStateHolderTo.ChoosenAgents.SingleOrDefault(x => x.Id.Value == lockInfo.AgentId);
				if (agent != null)
				{
					lockInfoForStateHolder.GridlockManager.AddLock(agent, lockInfo.Date, LockType.Normal);
				}
			}
		}

		private IEnumerable<ISkill> skillsToUse(IEnumerable<IPerson> agents, DateOnlyPeriod period, IEnumerable<Guid> onlyUseSkills)
		{
			var agentSkills = new HashSet<ISkill>();
			foreach (var agent in agents)
			{
				foreach (var personPeriod in agent.PersonPeriods(period))
				{
					//TODO: This will probably be wrong if we want to to shovel inside islands in upcoming PBIs
					foreach (var skill in _personalSkillsProvider.PersonSkills(personPeriod).Select(x => x.Skill))
					{
						if (onlyUseSkills==null || onlyUseSkills.Contains(skill.Id.Value))
						{
							agentSkills.Add(skill);
						}
					}
				}
			}

			return agentSkills;
		}

		private static void removeUnwantedAgents(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIdsToKeep)
		{
			if (agentIdsToKeep != null) //remove this when also scheduling is converted to "events"
			{
				foreach (var agent in schedulerStateHolderTo.ChoosenAgents.ToList().Where(agent => !agentIdsToKeep.Contains(agent.Id.Value)))
				{
					schedulerStateHolderTo.ChoosenAgents.Remove(agent);
				}
				foreach (var agent in schedulerStateHolderTo.SchedulingResultState.LoadedAgents.ToList().Where(agent => !agentIdsToKeep.Contains(agent.Id.Value)))
				{
					schedulerStateHolderTo.SchedulingResultState.LoadedAgents.Remove(agent);
				}
			}
		}

		private static void removeUnwantedSkillsAndSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<ISkill> skillsToKeep)
		{
			foreach (var skill in schedulerStateHolderTo.SchedulingResultState.Skills.ToList().Where(skill => !skillsToKeep.Contains(skill)))
			{
				schedulerStateHolderTo.SchedulingResultState.RemoveSkill(skill);
			}
			foreach (var skillDay in schedulerStateHolderTo.SchedulingResultState.SkillDays.ToList().Where(skillDay => !skillsToKeep.Contains(skillDay.Key)))
			{
				schedulerStateHolderTo.SchedulingResultState.SkillDays.Remove(skillDay.Key);
			}
			foreach (var emptySkillday in schedulerStateHolderTo.SchedulingResultState.SkillDays.ToList().Where(skillDay => !skillDay.Value.Any()))
			{
				schedulerStateHolderTo.SchedulingResultState.SkillDays.Remove(emptySkillday.Key);
				schedulerStateHolderTo.SchedulingResultState.RemoveSkill(emptySkillday.Key);
			}
			foreach (var skillWithMissingSkillDay in schedulerStateHolderTo.SchedulingResultState.Skills.ToList().Where(skill => !schedulerStateHolderTo.SchedulingResultState.SkillDays.Keys.Contains(skill)))
			{
				schedulerStateHolderTo.SchedulingResultState.RemoveSkill(skillWithMissingSkillDay);
			}
		}

		private static void removeUnwantedScheduleRanges(ISchedulerStateHolder schedulerStateHolderTo)
		{
			foreach (var person in schedulerStateHolderTo.Schedules.Keys.Where(person => !schedulerStateHolderTo.SchedulingResultState.LoadedAgents.Contains(person)).ToList())
			{
				schedulerStateHolderTo.Schedules.Remove(person);
			}
		}

		protected abstract IScenario FetchScenario();
		protected abstract void FillAgents(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIds, IEnumerable<Guid> choosenAgentIds, DateOnlyPeriod period);
		protected abstract void FillSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<ISkill> skills, DateOnlyPeriod period);
		protected abstract void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period);
		protected abstract void PreFill(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period);
		protected abstract void PostFill(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period);
	}
}