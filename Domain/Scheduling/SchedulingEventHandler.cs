using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingEventHandler
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IScheduleExecutor _scheduleExecutor;
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;
		private readonly ICurrentSchedulingCallback _currentSchedulingCallback;

		public SchedulingEventHandler(Func<ISchedulerStateHolder> schedulerStateHolder, 
						IScheduleExecutor scheduleExecutor, 
						ISchedulingOptionsProvider schedulingOptionsProvider,
						ICurrentSchedulingCallback currentSchedulingCallback)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleExecutor = scheduleExecutor;
			_schedulingOptionsProvider = schedulingOptionsProvider;
			_currentSchedulingCallback = currentSchedulingCallback;
		}

		public void HandleEvent(SchedulingWasOrdered @event)
		{
			var selectedPeriod = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
			var selectedAgents = _schedulerStateHolder().AllPermittedPersons.Where(x => @event.AgentsToSchedule.Contains(x.Id.Value)).ToArray();
			using (CommandScope.Create(@event))
			{
				//TODO: fix this later if needed
				var doWeNeedToFixThis = new NoSchedulingProgress();
				_scheduleExecutor.Execute(_currentSchedulingCallback.Current(), _schedulingOptionsProvider.Fetch(), doWeNeedToFixThis, selectedAgents,
					selectedPeriod, @event.RunWeeklyRestSolver);
			}
		}
	}
}