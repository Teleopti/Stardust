using System;
using System.Linq;
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

		public SchedulingEventHandler(Func<ISchedulerStateHolder> schedulerStateHolder,
						IFillSchedulerStateHolder fillSchedulerStateHolder,
						IScheduleExecutor scheduleExecutor, 
						ISchedulingOptionsProvider schedulingOptionsProvider,
						ICurrentSchedulingCallback currentSchedulingCallback,
						ISynchronizeSchedulesAfterIsland synchronizeSchedulesAfterIsland)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_scheduleExecutor = scheduleExecutor;
			_schedulingOptionsProvider = schedulingOptionsProvider;
			_currentSchedulingCallback = currentSchedulingCallback;
			_synchronizeSchedulesAfterIsland = synchronizeSchedulesAfterIsland;
		}

		public void Handle(SchedulingWasOrdered @event)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var selectedPeriod = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
			using (CommandScope.Create(@event))
			{
				//TODO: fix this
				LockInfoForStateHolder lockInfoForStateHolderFixThis = null;
				_fillSchedulerStateHolder.Fill(schedulerStateHolder, @event.AgentsToSchedule, lockInfoForStateHolderFixThis, selectedPeriod);
				//TODO: fix this
				var fixThis = new NoSchedulingProgress();
				_scheduleExecutor.Execute(_currentSchedulingCallback.Current(), _schedulingOptionsProvider.Fetch(), fixThis, schedulerStateHolder.AllPermittedPersons,
					selectedPeriod, @event.RunWeeklyRestSolver);
				_synchronizeSchedulesAfterIsland.Synchronize(schedulerStateHolder.Schedules, selectedPeriod);
			}
		}
	}
}