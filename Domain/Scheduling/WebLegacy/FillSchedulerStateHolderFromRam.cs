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
			_schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Where(x => agentIds.Contains(x.Id.Value))
				.ForEach(x => schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization.Add(x));

			_schedulerStateHolderFrom.AllPermittedPersons.Where(x => agentIds.Contains(x.Id.Value)).ForEach(x => schedulerStateHolderTo.AllPermittedPersons.Add(x));
			schedulerStateHolderTo.SchedulingResultState.AllPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>();

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

		public void Synchronize(IScheduleDictionary modifiedScheduleDictionary, DateOnlyPeriod period)
		{
			var agentsToMove = modifiedScheduleDictionary.Keys;
			moveSchedules(modifiedScheduleDictionary, _schedulerStateHolderFrom.Schedules, agentsToMove, period);
		}

		private static void moveSchedules(IScheduleDictionary fromDic, IScheduleDictionary toDic, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			foreach (var agent in agents)
			{
				var fromScheduleDays = fromDic[agent].ScheduledDayCollection(period);
				foreach (var fromScheduleDay in fromScheduleDays)
				{
					var toScheduleDay = toDic[agent].ScheduledDay(fromScheduleDay.DateOnlyAsPeriod.DateOnly);
					var toAssignment = toScheduleDay.PersonAssignment(true);
					toAssignment.SetActivitiesAndShiftCategoryFrom(fromScheduleDay.PersonAssignment(true));
					toDic.Modify(toScheduleDay);
				}
			}
		}
	}
}