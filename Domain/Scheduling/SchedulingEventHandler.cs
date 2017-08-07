using System;
using System.Linq;
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

		public SchedulingEventHandler(Func<ISchedulerStateHolder> schedulerStateHolder,
						IFillSchedulerStateHolder fillSchedulerStateHolder,
						IScheduleExecutor scheduleExecutor, 
						ISchedulingOptionsProvider schedulingOptionsProvider,
						ICurrentSchedulingCallback currentSchedulingCallback,
						ISynchronizeSchedulesAfterIsland synchronizeSchedulesAfterIsland,
						IGridlockManager gridlockManager)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_scheduleExecutor = scheduleExecutor;
			_schedulingOptionsProvider = schedulingOptionsProvider;
			_currentSchedulingCallback = currentSchedulingCallback;
			_synchronizeSchedulesAfterIsland = synchronizeSchedulesAfterIsland;
			_gridlockManager = gridlockManager;
		}

		[TestLog]
		public virtual void Handle(SchedulingWasOrdered @event)
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
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, @event.AgentsInIsland,
				new LockInfoForStateHolder(_gridlockManager, @event.UserLocks), selectedPeriod);

			var schedulingCallback = _currentSchedulingCallback.Current();
			var converter = schedulingCallback as IConvertSchedulingCallbackToSchedulingProgress;
			var schedulingProgress = converter == null ? new NoSchedulingProgress() : converter.Convert();

			_scheduleExecutor.Execute(schedulingCallback,
				_schedulingOptionsProvider.Fetch(schedulerStateHolder.CommonStateHolder.DefaultDayOffTemplate), schedulingProgress,
				schedulerStateHolder.AllPermittedPersons.Where(x => @event.AgentsToSchedule.Contains(x.Id.Value)).ToArray(),
				selectedPeriod, @event.RunWeeklyRestSolver);
		}
	}
}