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
		public virtual void Fill(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIds, DateOnlyPeriod period)
		{
			PreFill(schedulerStateHolderTo);
			var scenario = FetchScenario();
			FillAgents(schedulerStateHolderTo, scenario, agentIds, period);
			removeUnwantedAgents(schedulerStateHolderTo, agentIds);
			var skills = skillsToUse(schedulerStateHolderTo.AllPermittedPersons, period);
			FillSkillDays(schedulerStateHolderTo, scenario, skills, period);
			removeUnwantedSkillDays(schedulerStateHolderTo, skills);
			FillSchedules(schedulerStateHolderTo, scenario, schedulerStateHolderTo.AllPermittedPersons, period);
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

		private static void removeUnwantedAgents(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIds)
		{
			if (agentIds != null)
			{
				foreach (var agent in schedulerStateHolderTo.AllPermittedPersons.ToList().Where(agent => !agentIds.Contains(agent.Id.Value)))
				{
					schedulerStateHolderTo.AllPermittedPersons.Remove(agent);
				}
				foreach (var agent in schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization.ToList().Where(agent => !agentIds.Contains(agent.Id.Value)))
				{
					schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization.Remove(agent);
				}
			}
		}

		private static void removeUnwantedSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<ISkill> skillsToKeep)
		{
			var skillsToRemove = schedulerStateHolderTo.SchedulingResultState.Skills.Except(skillsToKeep);
			foreach (var skill in skillsToRemove)
			{
				schedulerStateHolderTo.SchedulingResultState.SkillDays.Remove(skill);
				schedulerStateHolderTo.SchedulingResultState.RemoveSkill(skill);
			}
		}

		private static void removeUnwantedScheduleRanges(ISchedulerStateHolder schedulerStateHolderTo)
		{
			foreach (var person in schedulerStateHolderTo.Schedules.Keys.Where(person => !schedulerStateHolderTo.AllPermittedPersons.Contains(person)).ToList())
			{
				schedulerStateHolderTo.Schedules.Remove(person);
			}
		}

		protected abstract IScenario FetchScenario();
		protected abstract void FillAgents(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<Guid> agentIds, DateOnlyPeriod period);
		protected abstract void FillSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<ISkill> skills, DateOnlyPeriod period);
		protected abstract void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period);
		protected abstract void PreFill(ISchedulerStateHolder schedulerStateHolderTo);
		protected abstract void PostFill(ISchedulerStateHolder schedulerStateHolder, IEnumerable<IPerson> agents, DateOnlyPeriod period);
	}
}