using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

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
			LockInfoForStateHolder lockInfoForStateHolder, 
			DateOnlyPeriod period, IEnumerable<Guid> onlyUseSkills)
		{
			PreFill(schedulerStateHolderTo, period);
			FillScenario(schedulerStateHolderTo);
			FillAgents(schedulerStateHolderTo, agentsInIsland, period);
			removeUnwantedAgents(schedulerStateHolderTo, agentsInIsland);
			var skills = skillsToUse(schedulerStateHolderTo.SchedulingResultState.LoadedAgents, period, onlyUseSkills);
			FillSkillDays(schedulerStateHolderTo, schedulerStateHolderTo.RequestedScenario, skills, period);
			FillBpos(schedulerStateHolderTo, skills, period);
			removeUnwantedSkillsAndSkillDays(schedulerStateHolderTo, skills);
			AddSkillDaysForReducedSkills(schedulerStateHolderTo, period);
			FillSchedules(schedulerStateHolderTo, schedulerStateHolderTo.RequestedScenario, schedulerStateHolderTo.SchedulingResultState.LoadedAgents, period);
			removeUnwantedScheduleRanges(schedulerStateHolderTo);
			setLocks(schedulerStateHolderTo, lockInfoForStateHolder);
		}
		
		private static void setLocks(ISchedulerStateHolder schedulerStateHolderTo, LockInfoForStateHolder lockInfoForStateHolder)
		{
			if (lockInfoForStateHolder == null)
				return;

			var choosenAgentLookup = schedulerStateHolderTo.ChoosenAgents.ToDictionary(a => a.Id.Value);
			foreach (var lockInfo in lockInfoForStateHolder.Locks)
			{
				if (choosenAgentLookup.TryGetValue(lockInfo.AgentId, out var agent) && agent != null)
				{
					lockInfoForStateHolder.GridlockManager.AddLock(agent, lockInfo.Date, lockInfo.LockType);
				}
			}
		}

		private IEnumerable<ISkill> skillsToUse(IEnumerable<IPerson> agents, DateOnlyPeriod period, IEnumerable<Guid> onlyUseSkills)
		{
			var skills = new HashSet<ISkill>();
			foreach (var agent in agents)
			{
				foreach (var personPeriod in agent.PersonPeriods(period))
				{
					//TODO: This will probably be wrong if we want to to shovel inside islands in upcoming PBIs
					foreach (var skill in _personalSkillsProvider.PersonSkills(personPeriod).Select(x => x.Skill))
					{
						if (onlyUseSkills==null || onlyUseSkills.Contains(skill.Id.Value))
						{
							skills.Add(skill);
						}
					}
				}
			}
			//This will lead to wrong behavior if/when skill is a proxy. Haven't seen it as an issue yet though...
			var childSkills = skills.OfType<IChildSkill>();
			//
			var multisiteSkills = childSkills.Select(c => c.ParentSkill).Distinct();
			return skills.Concat(multisiteSkills);
		}
		
		private static void removeUnwantedAgents(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIdsToKeep)
		{
			if (agentIdsToKeep != null)
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
			foreach (var skillDay in schedulerStateHolderTo.SchedulingResultState.SkillDays.ToList()
				.Where(skillDay => !skillsToKeep.Contains(skillDay.Key) || !skillDay.Value.Any()))
			{
				schedulerStateHolderTo.SchedulingResultState.SkillDays.Remove(skillDay.Key);
			}
			
			var skillsToRemove = new HashSet<ISkill>();
			foreach (var skill in schedulerStateHolderTo.SchedulingResultState.Skills
				.Where(skill => !skillsToKeep.Contains(skill) || !schedulerStateHolderTo.SchedulingResultState.SkillDays.Keys.Contains(skill)))
			{
				skillsToRemove.Add(skill);
			}
			var skillToKeepInStateHolder = schedulerStateHolderTo.SchedulingResultState.Skills.Except(skillsToRemove);
			schedulerStateHolderTo.SchedulingResultState.Skills = new HashSet<ISkill>(skillToKeepInStateHolder);
		}

		private static void removeUnwantedScheduleRanges(ISchedulerStateHolder schedulerStateHolderTo)
		{
			foreach (var person in schedulerStateHolderTo.Schedules.Keys.Where(person => !schedulerStateHolderTo.SchedulingResultState.LoadedAgents.Contains(person)).ToList())
			{
				schedulerStateHolderTo.Schedules.Remove(person);
			}
		}

		protected abstract void FillScenario(ISchedulerStateHolder schedulerStateHolderTo);
		protected abstract void FillAgents(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIds, DateOnlyPeriod period);
		protected abstract void FillSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<ISkill> skills, DateOnlyPeriod period);
		protected abstract void AddSkillDaysForReducedSkills(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period);
		protected abstract void FillBpos(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<ISkill> skills, DateOnlyPeriod period);
		protected abstract void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period);
		protected abstract void PreFill(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period);
	}
}