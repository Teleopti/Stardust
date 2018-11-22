using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourcePlanner;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	[InstancePerLifetimeScope]
	public class SchedulingEventHandler : IRunInSyncInFatClientProcess, IHandleEvent<SchedulingWasOrdered>
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly FillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly ScheduleExecutor _scheduleExecutor;
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;
		private readonly ICurrentSchedulingCallback _currentSchedulingCallback;
		private readonly ISynchronizeSchedulesAfterIsland _synchronizeSchedulesAfterIsland;
		private readonly IGridlockManager _gridlockManager;
		private readonly ISchedulingSourceScope _schedulingSourceScope;
		private readonly ExtendSelectedPeriodForMonthlyScheduling _extendSelectedPeriodForMonthlyScheduling;
		private readonly IBlockPreferenceProviderForPlanningPeriod _blockPreferenceProviderForPlanningPeriod;
		private readonly DayOffOptimization _dayOffOptimization;
		private readonly AlreadyScheduledAgents _alreadyScheduledAgents;
		private readonly AgentsWithPreferences _agentsWithPreferences;
		private readonly AgentsWithWhiteSpots _agentsWithWhiteSpots;
		private readonly RemoveNonPreferenceDaysOffs _removeNonPreferenceDaysOffs;
		private readonly IPlanningGroupProvider _planningGroupProvider;

		protected SchedulingEventHandler(Func<ISchedulerStateHolder> schedulerStateHolder,
						FillSchedulerStateHolder fillSchedulerStateHolder,
						ScheduleExecutor scheduleExecutor, 
						ISchedulingOptionsProvider schedulingOptionsProvider,
						ICurrentSchedulingCallback currentSchedulingCallback,
						ISynchronizeSchedulesAfterIsland synchronizeSchedulesAfterIsland,
						IGridlockManager gridlockManager, 
						ISchedulingSourceScope schedulingSourceScope,
						ExtendSelectedPeriodForMonthlyScheduling extendSelectedPeriodForMonthlyScheduling,
						IBlockPreferenceProviderForPlanningPeriod blockPreferenceProviderForPlanningPeriod,
						DayOffOptimization dayOffOptimization,
						AlreadyScheduledAgents alreadyScheduledAgents,
						AgentsWithPreferences agentsWithPreferences, 
						AgentsWithWhiteSpots agentsWithWhiteSpots,
						RemoveNonPreferenceDaysOffs removeNonPreferenceDaysOffs,
						IPlanningGroupProvider planningGroupProvider)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_scheduleExecutor = scheduleExecutor;
			_schedulingOptionsProvider = schedulingOptionsProvider;
			_currentSchedulingCallback = currentSchedulingCallback;
			_synchronizeSchedulesAfterIsland = synchronizeSchedulesAfterIsland;
			_gridlockManager = gridlockManager;
			_schedulingSourceScope = schedulingSourceScope;
			_extendSelectedPeriodForMonthlyScheduling = extendSelectedPeriodForMonthlyScheduling;
			_blockPreferenceProviderForPlanningPeriod = blockPreferenceProviderForPlanningPeriod;
			_dayOffOptimization = dayOffOptimization;
			_alreadyScheduledAgents = alreadyScheduledAgents;
			_agentsWithPreferences = agentsWithPreferences;
			_agentsWithWhiteSpots = agentsWithWhiteSpots;
			_removeNonPreferenceDaysOffs = removeNonPreferenceDaysOffs;
			_planningGroupProvider = planningGroupProvider;
		}

		[TestLog]
		public virtual void Handle(SchedulingWasOrdered @event)
		{
			using (_schedulingSourceScope.OnThisThreadUse(@event.FromWeb ? ScheduleSource.WebScheduling : null))
			{
				var schedulerStateHolder = _schedulerStateHolder();
				var selectedPeriod = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
				using (CommandScope.Create(@event))
				{
					DoScheduling(@event, schedulerStateHolder, selectedPeriod);
				
					_synchronizeSchedulesAfterIsland.Synchronize(schedulerStateHolder.Schedules, selectedPeriod);
				}
			}
		}

		[ReadonlyUnitOfWork]
		protected virtual void DoScheduling(SchedulingWasOrdered @event, ISchedulerStateHolder schedulerStateHolder, DateOnlyPeriod selectedPeriod)
		{
			var planningGroup = _planningGroupProvider.Execute(@event.PlanningPeriodId);
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, @event.AgentsInIsland, new LockInfoForStateHolder(_gridlockManager, @event.UserLocks), selectedPeriod, @event.Skills);
			var schedulingCallback = _currentSchedulingCallback.Current();
			var schedulingProgress = schedulingCallback is IConvertSchedulingCallbackToSchedulingProgress converter ? converter.Convert() : new NoSchedulingProgress();
			var schedulingOptions = _schedulingOptionsProvider.Fetch(schedulerStateHolder.CommonStateHolder.DefaultDayOffTemplate);
			var blockPreferenceProvider = _blockPreferenceProviderForPlanningPeriod.Fetch(planningGroup);
			selectedPeriod = _extendSelectedPeriodForMonthlyScheduling.Execute(@event, schedulerStateHolder, selectedPeriod);
			var agents = schedulerStateHolder.SchedulingResultState.LoadedAgents.Where(x => @event.Agents.Contains(x.Id.Value)).ToArray();
			var agentsWithExistingShiftsBeforeSchedule = _alreadyScheduledAgents.Execute(schedulerStateHolder.Schedules, selectedPeriod, agents);
			_scheduleExecutor.Execute(schedulingCallback, schedulingOptions, schedulingProgress, agents, selectedPeriod, blockPreferenceProvider);

			if (runSchedulingWithoutPreferences(agentsWithExistingShiftsBeforeSchedule, @event, agents, selectedPeriod,
				schedulingOptions, schedulingCallback, schedulingProgress, blockPreferenceProvider))
			{
				//to be fixed later - just hack for now
				planningGroup.SetGlobalValues(Percent.Zero);
			}
		
			if (schedulingOptions.PreferencesDaysOnly || schedulingOptions.UsePreferencesMustHaveOnly)
			{
				_removeNonPreferenceDaysOffs.Execute(schedulerStateHolder.Schedules, agents, selectedPeriod);
			}
			
			if (@event.RunDayOffOptimization)
			{
				_dayOffOptimization.Execute(new DateOnlyPeriod(@event.StartDate, @event.EndDate),
					agents,
					true,
					planningGroup);
			}
		}

		private bool runSchedulingWithoutPreferences(
			IDictionary<IPerson, IEnumerable<DateOnly>> alreadyScheduledAgents, SchedulingWasOrdered @event,IEnumerable<IPerson> agents,
			DateOnlyPeriod selectedPeriod, SchedulingOptions schedulingOptions,
			ISchedulingCallback schedulingCallback, ISchedulingProgress schedulingProgress,
			IBlockPreferenceProvider blockPreferenceProvider)
		{
			if (!@event.ScheduleWithoutPreferencesForFailedAgents) 
				return false;
			var schedules = _schedulerStateHolder().Schedules;
			var agentsWithPreferences = _agentsWithPreferences.Execute(schedules, agents, selectedPeriod);
			var filteredAgents = _agentsWithWhiteSpots.Execute(schedules, agentsWithPreferences, selectedPeriod);

			foreach (var agent in filteredAgents)
			{
				var range = schedules[agent];
				foreach (var date in selectedPeriod.DayCollection())
				{
					if(alreadyScheduledAgents.TryGetValue(agent, out var alreadyScheduleDates) && alreadyScheduleDates.Contains(date))
						continue;
						
					var scheduleDay = range.ScheduledDay(date);
					scheduleDay.PersonAssignment(true).ClearMainActivities();
					scheduleDay.PersonAssignment().SetDayOff(null);

					//Correct, res calc numbers?!?!?
					schedules.Modify(scheduleDay, new DoNothingScheduleDayChangeCallBack());
				}
			}
			schedulingOptions.UsePreferences = false;
			_scheduleExecutor.Execute(schedulingCallback, schedulingOptions, schedulingProgress, filteredAgents, selectedPeriod, blockPreferenceProvider);
			return filteredAgents.Any();
		}
	}
}