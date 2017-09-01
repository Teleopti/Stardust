using System;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingEventHandler : IRunInSyncInFatClientProcess, IHandleEvent<SchedulingWasOrdered>
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IFillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly IScheduleExecutor _scheduleExecutor;
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;
		private readonly ICurrentSchedulingCallback _currentSchedulingCallback;
		private readonly ISynchronizeSchedulesAfterIsland _synchronizeSchedulesAfterIsland;
		private readonly IGridlockManager _gridlockManager;
		private readonly ISchedulingSourceScope _schedulingSourceScope;

		public SchedulingEventHandler(Func<ISchedulerStateHolder> schedulerStateHolder,
						IFillSchedulerStateHolder fillSchedulerStateHolder,
						IScheduleExecutor scheduleExecutor, 
						ISchedulingOptionsProvider schedulingOptionsProvider,
						ICurrentSchedulingCallback currentSchedulingCallback,
						ISynchronizeSchedulesAfterIsland synchronizeSchedulesAfterIsland,
						IGridlockManager gridlockManager, 
						ISchedulingSourceScope schedulingSourceScope)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_scheduleExecutor = scheduleExecutor;
			_schedulingOptionsProvider = schedulingOptionsProvider;
			_currentSchedulingCallback = currentSchedulingCallback;
			_synchronizeSchedulesAfterIsland = synchronizeSchedulesAfterIsland;
			_gridlockManager = gridlockManager;
			_schedulingSourceScope = schedulingSourceScope;
		}

		[TestLog]
		public virtual void Handle(SchedulingWasOrdered @event)
		{
			if (@event.FromWeb)
			{
				Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
				using (_schedulingSourceScope.OnThisThreadUse(ScheduleSource.WebScheduling))
				{
					Run(@event);
				}
				Thread.CurrentThread.Priority = ThreadPriority.Normal;
			}
			else
			{
				Run(@event);
			}
		}

		private void Run(SchedulingWasOrdered @event)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var selectedPeriod = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
			using (CommandScope.Create(@event))
			{
				DoScheduling(@event, schedulerStateHolder, selectedPeriod);
				_synchronizeSchedulesAfterIsland.Synchronize(schedulerStateHolder.Schedules, selectedPeriod);
			}
		}

		[UnitOfWork]
		protected virtual void DoScheduling(SchedulingWasOrdered @event, ISchedulerStateHolder schedulerStateHolder, DateOnlyPeriod selectedPeriod)
		{
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, @event.AgentsInIsland, new LockInfoForStateHolder(_gridlockManager, @event.UserLocks), selectedPeriod, @event.Skills);
			var schedulingCallback = _currentSchedulingCallback.Current();
			var schedulingProgress = schedulingCallback is IConvertSchedulingCallbackToSchedulingProgress converter ? converter.Convert() : new NoSchedulingProgress();

			_scheduleExecutor.Execute(schedulingCallback,
				_schedulingOptionsProvider.Fetch(schedulerStateHolder.CommonStateHolder.DefaultDayOffTemplate), schedulingProgress,
				schedulerStateHolder.AllPermittedPersons.Where(x => @event.AgentsToSchedule.Contains(x.Id.Value)).ToArray(),
				selectedPeriod, @event.RunWeeklyRestSolver);
		}
	}
}