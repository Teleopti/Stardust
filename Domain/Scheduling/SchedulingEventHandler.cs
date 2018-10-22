using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourcePlanner;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	[InstancePerLifetimeScope]
	[EnabledBy(Toggles.ResourcePlanner_SeamlessPlanningForPreferences_76288)]
	[RemoveMeWithToggle("merge with base class", Toggles.ResourcePlanner_SeamlessPlanningForPreferences_76288)]
	public class SchedulingEventHandlerNew : SchedulingEventHandler
	{
		private readonly AgentsWithPreferences _agentsWithPreferences;
		private readonly AgentsWithWhiteSpots _agentsWithWhiteSpots;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly ScheduleExecutor _scheduleExecutor;
		
		public SchedulingEventHandlerNew(AgentsWithPreferences agentsWithPreferences, AgentsWithWhiteSpots agentsWithWhiteSpots, Func<ISchedulerStateHolder> schedulerStateHolder, FillSchedulerStateHolder fillSchedulerStateHolder, ScheduleExecutor scheduleExecutor, ISchedulingOptionsProvider schedulingOptionsProvider, ICurrentSchedulingCallback currentSchedulingCallback, ISynchronizeSchedulesAfterIsland synchronizeSchedulesAfterIsland, IGridlockManager gridlockManager, ISchedulingSourceScope schedulingSourceScope, ExtendSelectedPeriodForMonthlyScheduling extendSelectedPeriodForMonthlyScheduling, IBlockPreferenceProviderForPlanningPeriod blockPreferenceProviderForPlanningPeriod, DayOffOptimization dayOffOptimization, IAlreadyScheduledAgents alreadyScheduledAgents) : base(schedulerStateHolder, fillSchedulerStateHolder, scheduleExecutor, schedulingOptionsProvider, currentSchedulingCallback, synchronizeSchedulesAfterIsland, gridlockManager, schedulingSourceScope, extendSelectedPeriodForMonthlyScheduling, blockPreferenceProviderForPlanningPeriod, dayOffOptimization, alreadyScheduledAgents)
		{
			_agentsWithPreferences = agentsWithPreferences;
			_agentsWithWhiteSpots = agentsWithWhiteSpots;
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleExecutor = scheduleExecutor;
		}

		protected override void RunSchedulingWithoutPreferences(
			IDictionary<IPerson, IEnumerable<DateOnly>> alreadyScheduledAgents, SchedulingWasOrdered @event, IEnumerable<IPerson> agents,
			DateOnlyPeriod selectedPeriod, SchedulingOptions schedulingOptions, ISchedulingCallback schedulingCallback,
			ISchedulingProgress schedulingProgress, IBlockPreferenceProvider blockPreferenceProvider)
		{
			if (!@event.ScheduleWithoutPreferencesForFailedAgents) 
				return;
			var schedules = _schedulerStateHolder().Schedules;
			var agentsWithPreferences = _agentsWithPreferences.Execute(schedules, agents, selectedPeriod);
			var agentsWithWhiteSpotsAndPreferences = _agentsWithWhiteSpots.Execute(schedules, agentsWithPreferences, selectedPeriod);
			
			foreach (var agent in agentsWithWhiteSpotsAndPreferences)
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
			_scheduleExecutor.Execute(schedulingCallback, schedulingOptions, schedulingProgress, agentsWithWhiteSpotsAndPreferences,
				selectedPeriod, blockPreferenceProvider);
		}
	}
	
	
	[InstancePerLifetimeScope]
	[DisabledBy(Toggles.ResourcePlanner_SeamlessPlanningForPreferences_76288)]
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
		private readonly IAlreadyScheduledAgents _alreadyScheduledAgents;

		public SchedulingEventHandler(Func<ISchedulerStateHolder> schedulerStateHolder,
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
						IAlreadyScheduledAgents alreadyScheduledAgents)
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
		}

		[TestLog]
		public virtual void Handle(SchedulingWasOrdered @event)
		{
			if (@event.FromWeb)
			{
				using (_schedulingSourceScope.OnThisThreadUse(ScheduleSource.WebScheduling))
				{
					run(@event);
				}
			}
			else
			{
				run(@event);
			}
		}

		private void run(SchedulingWasOrdered @event)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var selectedPeriod = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
			using (CommandScope.Create(@event))
			{
				DoScheduling(@event, schedulerStateHolder, selectedPeriod);
				
				_synchronizeSchedulesAfterIsland.Synchronize(schedulerStateHolder.Schedules, selectedPeriod);
			}
		}

		[ReadonlyUnitOfWork]
		protected virtual void DoScheduling(SchedulingWasOrdered @event, ISchedulerStateHolder schedulerStateHolder, DateOnlyPeriod selectedPeriod)
		{
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, @event.AgentsInIsland, new LockInfoForStateHolder(_gridlockManager, @event.UserLocks), selectedPeriod, @event.Skills);
			var schedulingCallback = _currentSchedulingCallback.Current();
			var schedulingProgress = schedulingCallback is IConvertSchedulingCallbackToSchedulingProgress converter ? converter.Convert() : new NoSchedulingProgress();

			var schedulingOptions = _schedulingOptionsProvider.Fetch(schedulerStateHolder.CommonStateHolder.DefaultDayOffTemplate);

			var blockPreferenceProvider = @event.FromWeb ? 
				_blockPreferenceProviderForPlanningPeriod.Fetch(@event.PlanningPeriodId) : 
				new FixedBlockPreferenceProvider(schedulingOptions);
			selectedPeriod = _extendSelectedPeriodForMonthlyScheduling.Execute(@event, schedulerStateHolder, selectedPeriod);
			var agents = schedulerStateHolder.SchedulingResultState.LoadedAgents.Where(x => @event.Agents.Contains(x.Id.Value)).ToArray();

			var alreadyScheduledAgents = _alreadyScheduledAgents.Execute(schedulerStateHolder.Schedules, selectedPeriod, agents);
			
			_scheduleExecutor.Execute(schedulingCallback, schedulingOptions, schedulingProgress, agents, selectedPeriod, blockPreferenceProvider);
			
			RunSchedulingWithoutPreferences(alreadyScheduledAgents, @event, agents, selectedPeriod, schedulingOptions, schedulingCallback, schedulingProgress, blockPreferenceProvider);

			removeNonPreferenceDaysOffs(selectedPeriod, schedulingOptions, agents);
			
			if(@event.RunDayOffOptimization)
			{
				_dayOffOptimization.Execute(new DateOnlyPeriod(@event.StartDate, @event.EndDate),
					agents,
					true,
					@event.PlanningPeriodId);
			}
		}

		private void removeNonPreferenceDaysOffs(DateOnlyPeriod selectedPeriod, SchedulingOptions schedulingOptions, IEnumerable<IPerson> agents)
		{
			if (!schedulingOptions.PreferencesDaysOnly && !schedulingOptions.UsePreferencesMustHaveOnly) return;
			var schedules = _schedulerStateHolder().Schedules;

			foreach (var agent in agents)
			{
				var range = schedules[agent];
				foreach (var date in selectedPeriod.DayCollection())
				{
					var scheduleDay = range.ScheduledDay(date);
					if (scheduleDay.HasDayOff() && (scheduleDay.PreferenceDay() == null))
					{
						scheduleDay.DeleteDayOff();
					}

					schedules.Modify(scheduleDay, new DoNothingScheduleDayChangeCallBack());
				}
			}
		}

		protected virtual void RunSchedulingWithoutPreferences(
			IDictionary<IPerson, IEnumerable<DateOnly>> alreadyScheduledAgents, SchedulingWasOrdered @event,IEnumerable<IPerson> agents,
			DateOnlyPeriod selectedPeriod, SchedulingOptions schedulingOptions,
			ISchedulingCallback schedulingCallback, ISchedulingProgress schedulingProgress,
			IBlockPreferenceProvider blockPreferenceProvider)
		{
		}
	}
}