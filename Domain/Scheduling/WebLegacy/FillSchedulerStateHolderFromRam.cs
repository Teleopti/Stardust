using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class FillSchedulerStateHolderFromRam : IFillSchedulerStateHolder, ISynchronizeIntradayOptimizationResult
	{
		private ISchedulerStateHolder _schedulerStateHolderFrom;

		public WebSchedulingSetupResult Fill(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIds, DateOnlyPeriod period)
		{
			_schedulerStateHolderFrom.AllPermittedPersons.Where(x => agentIds.Contains(x.Id.Value)).ForEach(x => schedulerStateHolderTo.AllPermittedPersons.Add(x));

			var scheduleDictionary = new ScheduleDictionary(_schedulerStateHolderFrom.Schedules.Scenario, _schedulerStateHolderFrom.Schedules.Period);
			moveSchedules(_schedulerStateHolderFrom.Schedules, scheduleDictionary, schedulerStateHolderTo.AllPermittedPersons, period);
			schedulerStateHolderTo.SchedulingResultState.Schedules = scheduleDictionary;
			schedulerStateHolderTo.SchedulingResultState.SkillDays = _schedulerStateHolderFrom.SchedulingResultState.SkillDays;

			return null;
		}

		public IDisposable Add(ISchedulerStateHolder schedulerStateHolderFrom)
		{
			_schedulerStateHolderFrom = schedulerStateHolderFrom;
			return new GenericDisposable(() => _schedulerStateHolderFrom = null);
		}

		public void Execute(IScheduleDictionary modifiedScheduleDictionary)
		{
			var agentsToMove = modifiedScheduleDictionary.Keys;
			//don't remove old data here (in other periods)!
			agentsToMove.ForEach(x => _schedulerStateHolderFrom.Schedules.Remove(x));
			//get real period here instead
			var hack = new DateOnlyPeriod(new DateOnly(modifiedScheduleDictionary.Period.VisiblePeriod.StartDateTime.AddDays(-1)), new DateOnly(modifiedScheduleDictionary.Period.VisiblePeriod.EndDateTime.AddDays(1)));

			moveSchedules(modifiedScheduleDictionary, _schedulerStateHolderFrom.Schedules, agentsToMove, hack);
		}

		private static void moveSchedules(IScheduleDictionary fromDic, IScheduleDictionary toDic, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			foreach (var agent in agents)
			{
				fromDic[agent].ScheduledDayCollection(period).ForEach(x => toDic.Modify(x));
			}
		}
	}
}