using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public abstract class FillSchedulerStateHolder : IFillSchedulerStateHolder
	{
		[LogTime]
		public virtual void Fill(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentsInIsland, DateOnlyPeriod period)
		{
			PreFill(schedulerStateHolderTo, period);
			var scenario = FetchScenario();
			FillAgents(schedulerStateHolderTo, agentsInIsland, period);
			removeUnwantedAgents(schedulerStateHolderTo, agentsInIsland);
			var skills = skillsToUse(schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization, period);
			FillSkillDays(schedulerStateHolderTo, scenario, skills, period);
			removeUnwantedSkillDays(schedulerStateHolderTo, skills);
			FillSchedules(schedulerStateHolderTo, scenario, schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization, period);
			removeUnwantedScheduleRanges(schedulerStateHolderTo);
			PostFill(schedulerStateHolderTo, schedulerStateHolderTo.AllPermittedPersons, period);
		}

		private static IEnumerable<ISkill> skillsToUse(IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var agentSkills = new HashSet<ISkill>();
			foreach (var skill in agents.SelectMany(filteredAgent => filteredAgent.ActiveSkillsFor(period)))
			{
				agentSkills.Add(skill);
			}
			return agentSkills;
		}

		private static void removeUnwantedAgents(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIdsToKeep)
		{
			if (agentIdsToKeep != null) //remove this when also scheduling is converted to "events"
			{
				foreach (var agent in schedulerStateHolderTo.AllPermittedPersons.ToList().Where(agent => !agentIdsToKeep.Contains(agent.Id.Value)))
				{
					schedulerStateHolderTo.AllPermittedPersons.Remove(agent);
				}
				foreach (var agent in schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization.ToList().Where(agent => !agentIdsToKeep.Contains(agent.Id.Value)))
				{
					schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization.Remove(agent);
				}
			}
		}

		private static void removeUnwantedSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<ISkill> skillsToKeep)
		{
			foreach (var skill in schedulerStateHolderTo.SchedulingResultState.Skills.ToList().Where(skill => !skillsToKeep.Contains(skill)))
			{
				schedulerStateHolderTo.SchedulingResultState.RemoveSkill(skill);
			}
			foreach (var skillDay in schedulerStateHolderTo.SchedulingResultState.SkillDays.ToList().Where(skillDay => !skillsToKeep.Contains(skillDay.Key)))
			{
				schedulerStateHolderTo.SchedulingResultState.SkillDays.Remove(skillDay.Key);
			}
		}

		private static void removeUnwantedScheduleRanges(ISchedulerStateHolder schedulerStateHolderTo)
		{
			foreach (var person in schedulerStateHolderTo.Schedules.Keys.Where(person => !schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization.Contains(person)).ToList())
			{
				schedulerStateHolderTo.Schedules.Remove(person);
			}
		}

		protected abstract IScenario FetchScenario();
		protected abstract void FillAgents(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIds, DateOnlyPeriod period);
		protected abstract void FillSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<ISkill> skills, DateOnlyPeriod period);
		protected abstract void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period);
		protected abstract void PreFill(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period);
		protected abstract void PostFill(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<IPerson> agents, DateOnlyPeriod period);
	}
}