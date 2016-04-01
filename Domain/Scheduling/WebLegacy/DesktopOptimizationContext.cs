using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class DesktopOptimizationContext : FillSchedulerStateHolder, ISynchronizeIntradayOptimizationResult, IOptimizationPreferencesProvider, IPeopleInOrganization, ICurrentIntradayOptimizationCallback
	{
		private class desktopOptimizationContextData
		{
			public desktopOptimizationContextData(ISchedulerStateHolder schedulerStateHolderFrom, IOptimizationPreferences optimizationPreferences, IIntradayOptimizationCallback intradayOptimizationCallback)
			{
				SchedulerStateHolderFrom = schedulerStateHolderFrom;
				OptimizationPreferences = optimizationPreferences;
				IntradayOptimizationCallback = intradayOptimizationCallback;
			}

			public ISchedulerStateHolder SchedulerStateHolderFrom { get; private set; }
			public IOptimizationPreferences OptimizationPreferences { get; private set; }
			public IIntradayOptimizationCallback IntradayOptimizationCallback { get; private set; }
		}
		private readonly ConcurrentDictionary<Guid, desktopOptimizationContextData> _contextPerCommand = new ConcurrentDictionary<Guid, desktopOptimizationContextData>();

		private ISchedulerStateHolder schedulerStateHolderFrom()
		{
			return _contextPerCommand[CommandScope.Current().CommandId].SchedulerStateHolderFrom;
		}

		protected override IScenario FetchScenario()
		{
			return schedulerStateHolderFrom().Schedules.Scenario;
		}

		protected override void FillAgents(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<Guid> agentIds, DateOnlyPeriod period)
		{
			var stateHolderFrom = schedulerStateHolderFrom();
			stateHolderFrom.SchedulingResultState.PersonsInOrganization.Where(x => agentIds.Contains(x.Id.Value))
				.ForEach(x => schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization.Add(x));
			stateHolderFrom.AllPermittedPersons.Where(x => agentIds.Contains(x.Id.Value))
				.ForEach(x => schedulerStateHolderTo.AllPermittedPersons.Add(x));
		}

		protected override void FillSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<ISkill> skills, DateOnlyPeriod period)
		{
			var stateHolderFrom = schedulerStateHolderFrom();
			schedulerStateHolderTo.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>(stateHolderFrom.SchedulingResultState.SkillDays);
			schedulerStateHolderTo.SchedulingResultState.AddSkills(skills.ToArray());
		}

		protected override void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var stateHolderFrom = schedulerStateHolderFrom();
			var scheduleDictionary = new ScheduleDictionary(scenario, stateHolderFrom.Schedules.Period);
			using (TurnoffPermissionScope.For(scheduleDictionary))
			{
				moveSchedules(stateHolderFrom.Schedules, scheduleDictionary, schedulerStateHolderTo.AllPermittedPersons,
					stateHolderFrom.Schedules.Period.LoadedPeriod().ToDateOnlyPeriod(stateHolderFrom.TimeZoneInfo), true);
			}
			schedulerStateHolderTo.SchedulingResultState.Schedules = scheduleDictionary;
		}

		protected override void PreFill(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period)
		{
			schedulerStateHolderTo.SchedulingResultState.AllPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>();
		}

		protected override void PostFill(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			schedulerStateHolderTo.RequestedPeriod = schedulerStateHolderFrom().RequestedPeriod;
		}

		public void Synchronize(IScheduleDictionary modifiedScheduleDictionary, DateOnlyPeriod period)
		{
			var agentsToMove = modifiedScheduleDictionary.Keys;
			moveSchedules(modifiedScheduleDictionary, schedulerStateHolderFrom().Schedules, agentsToMove, period, false);
		}

		public IDisposable Set(ICommandIdentifier commandIdentifier, ISchedulerStateHolder schedulerStateHolderFrom, IOptimizationPreferences optimizationPreferences, IIntradayOptimizationCallback intradayOptimizationCallback)
		{
			_contextPerCommand[commandIdentifier.CommandId] = new desktopOptimizationContextData(schedulerStateHolderFrom, optimizationPreferences, intradayOptimizationCallback);
			return new GenericDisposable(() =>
			{
				desktopOptimizationContextData foo;
				_contextPerCommand.TryRemove(commandIdentifier.CommandId, out foo);
			});
		}

		private static void moveSchedules(IScheduleDictionary fromDic, 
														IScheduleDictionary toDic, 
														IEnumerable<IPerson> agents, 
														DateOnlyPeriod period,
														bool includeNonAssignments)
		{
			foreach (var agent in agents)
			{
				var fromScheduleDays = fromDic[agent].ScheduledDayCollection(period);
				foreach (var fromScheduleDay in fromScheduleDays)
				{
					var toScheduleDay = toDic[agent].ScheduledDay(fromScheduleDay.DateOnlyAsPeriod.DateOnly);	
					var toAssignment = toScheduleDay.PersonAssignment(true);
					toAssignment.FillWithDataFrom(fromScheduleDay.PersonAssignment(true));

					if (includeNonAssignments)
					{
						fromScheduleDay.PersistableScheduleDataCollection().OfType<IPersonAbsence>().ForEach(x => toScheduleDay.Add(x));
						fromScheduleDay.PersonMeetingCollection().ForEach(x => ((ScheduleRange)toDic[agent]).Add(x));
						fromScheduleDay.PersonRestrictionCollection().ForEach(x => ((ScheduleRange)toDic[agent]).Add(x));
						fromScheduleDay.PersistableScheduleDataCollection().OfType<IPreferenceDay>().ForEach(x => toScheduleDay.Add(x));
					}

					toDic.Modify(toScheduleDay);
				}
			}
		}

		public IOptimizationPreferences Fetch()
		{
			return _contextPerCommand[CommandScope.Current().CommandId].OptimizationPreferences;
		}

		public IEnumerable<IPerson> Agents(DateOnlyPeriod period)
		{
			return schedulerStateHolderFrom().SchedulingResultState.PersonsInOrganization;
		}

		public IIntradayOptimizationCallback Current()
		{
			return _contextPerCommand[CommandScope.Current().CommandId].IntradayOptimizationCallback;
		}
	}
}