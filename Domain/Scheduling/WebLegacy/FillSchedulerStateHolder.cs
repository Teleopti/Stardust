using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public abstract class FillSchedulerStateHolder : IFillSchedulerStateHolder
	{
		public void Fill(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIds, DateOnlyPeriod period)
		{
			PreFill(schedulerStateHolderTo);
			var scenario = FetchScenario();
			var agents = FillAgents(schedulerStateHolderTo, scenario, agentIds, period);
			var filteredAgents = agents.Filter(agentIds).ToList();
			FillSkillDays(schedulerStateHolderTo, scenario, filteredAgents, period);
			removeUnwantedSkillDays(schedulerStateHolderTo, filteredAgents, period);
			FillSchedules(schedulerStateHolderTo, scenario, filteredAgents, period);
			removeUnwantedScheduleRanges(schedulerStateHolderTo.Schedules, filteredAgents);
			PostFill(schedulerStateHolderTo, filteredAgents, period);
		}

		private static void removeUnwantedSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<IPerson> filteredAgents, DateOnlyPeriod period)
		{
			var agentSkills = new HashSet<ISkill>();
			foreach (var filteredAgent in filteredAgents)
			{
				filteredAgent.ActiveSkillsFor(period).ForEach(x => agentSkills.Add(x));
			}

			foreach (var skill in schedulerStateHolderTo.SchedulingResultState.SkillDays.Keys.ToList().Where(skill => !agentSkills.Contains(skill)))
			{
				schedulerStateHolderTo.SchedulingResultState.SkillDays.Remove(skill);
			}
		}

		private static void removeUnwantedScheduleRanges(IScheduleDictionary schedules, IEnumerable<IPerson> filteredAgents)
		{
			foreach (var person in schedules.Keys.Where(person => !filteredAgents.Contains(person)).ToList())
			{
				schedules.Remove(person);
			}
		}

		protected abstract IScenario FetchScenario();
		protected abstract IEnumerable<IPerson> FillAgents(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<Guid> agentIds, DateOnlyPeriod period);
		protected abstract void FillSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period);
		protected abstract void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period);
		protected abstract void PreFill(ISchedulerStateHolder schedulerStateHolderTo);
		protected abstract void PostFill(ISchedulerStateHolder schedulerStateHolder, IEnumerable<IPerson> agents, DateOnlyPeriod period);
	}
}