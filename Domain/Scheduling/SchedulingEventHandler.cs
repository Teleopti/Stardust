using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
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
		private readonly FailedScheduledAgents _failedScheduledAgents;

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
						FailedScheduledAgents failedScheduledAgents)
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
			_failedScheduledAgents = failedScheduledAgents;
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
			
			_scheduleExecutor.Execute(schedulingCallback, schedulingOptions, schedulingProgress, agents, selectedPeriod, blockPreferenceProvider);

			if (@event.FromWeb)
			{
				var failedScheduleAgents = _failedScheduledAgents.Execute(schedulerStateHolder.Schedules, selectedPeriod).Where(x => @event.Agents.Contains(x.Id.Value));
				// below needs to be handled differently if/when DO should use pref (this line affects that as well)
				schedulingOptions.UsePreferences = false;
				_scheduleExecutor.Execute(schedulingCallback, schedulingOptions, schedulingProgress, failedScheduleAgents, selectedPeriod, blockPreferenceProvider);
			}
			
			if(@event.RunDayOffOptimization)
			{
				_dayOffOptimization.Execute(new DateOnlyPeriod(@event.StartDate, @event.EndDate),
					agents,
					true,
					@event.PlanningPeriodId);
			}
		}
	}
}