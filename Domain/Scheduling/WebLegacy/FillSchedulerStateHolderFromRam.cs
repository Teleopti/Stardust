using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class FillSchedulerStateHolderFromRam : FillSchedulerStateHolder, ISynchronizeIntradayOptimizationResult
	{
		private ISchedulerStateHolder _schedulerStateHolderFrom;

		protected override IScenario FetchScenario()
		{
			return _schedulerStateHolderFrom.Schedules.Scenario;
		}

		protected override IEnumerable<IPerson> FillAgents(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<Guid> agentIds, DateOnlyPeriod period)
		{
			_schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Where(x => agentIds.Contains(x.Id.Value))
							.ForEach(x => schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization.Add(x));
			_schedulerStateHolderFrom.AllPermittedPersons.Where(x => agentIds.Contains(x.Id.Value)).ForEach(x => schedulerStateHolderTo.AllPermittedPersons.Add(x));
			return schedulerStateHolderTo.AllPermittedPersons;
		}

		protected override void FillSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			schedulerStateHolderTo.SchedulingResultState.SkillDays = _schedulerStateHolderFrom.SchedulingResultState.SkillDays;
		}

		protected override void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var scheduleDictionary = new ScheduleDictionary(scenario, _schedulerStateHolderFrom.Schedules.Period);
			moveSchedules(_schedulerStateHolderFrom.Schedules, scheduleDictionary, schedulerStateHolderTo.AllPermittedPersons, _schedulerStateHolderFrom.Schedules.Period.LoadedPeriod().ToDateOnlyPeriod(_schedulerStateHolderFrom.TimeZoneInfo));
			schedulerStateHolderTo.SchedulingResultState.Schedules = scheduleDictionary;
		}

		protected override void PreFill(ISchedulerStateHolder schedulerStateHolderTo)
		{
			schedulerStateHolderTo.SchedulingResultState.AllPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>();
		}

		protected override void PostFill(ISchedulerStateHolder schedulerStateHolder, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
		}

		public void Synchronize(IScheduleDictionary modifiedScheduleDictionary, DateOnlyPeriod period)
		{
			var agentsToMove = modifiedScheduleDictionary.Keys;
			moveSchedules(modifiedScheduleDictionary, _schedulerStateHolderFrom.Schedules, agentsToMove, period);
		}

		public IDisposable Add(ISchedulerStateHolder schedulerStateHolderFrom)
		{
			_schedulerStateHolderFrom = schedulerStateHolderFrom;
			return new GenericDisposable(() => _schedulerStateHolderFrom = null);
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
					toAssignment.FillWithDataFrom(fromScheduleDay.PersonAssignment(true));

					var fromAbsences = fromScheduleDay.PersistableScheduleDataCollection().OfType<IPersonAbsence>();
					foreach (var personAbsence in fromAbsences)
					{
						toScheduleDay.Add(personAbsence);
					}

					var fromMeetings = fromScheduleDay.PersonMeetingCollection();
					foreach (var personMeeting in fromMeetings)
					{
						((ScheduleRange)toDic[agent]).Add(personMeeting);
					}

					toDic.Modify(toScheduleDay);
				}
			}
		}
	}
}