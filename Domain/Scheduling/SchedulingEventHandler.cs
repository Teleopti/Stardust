using System;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingEventHandler
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IScheduleExecutor _scheduleExecutor;
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;

		public SchedulingEventHandler(Func<ISchedulerStateHolder> schedulerStateHolder, IScheduleExecutor scheduleExecutor, ISchedulingOptionsProvider schedulingOptionsProvider)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleExecutor = scheduleExecutor;
			_schedulingOptionsProvider = schedulingOptionsProvider;
		}

		public void HandleEvent(SchedulingWasOrdered @event,
			//remove these later
			ISchedulingCallback schedulingCallback,
			ISchedulingProgress backgroundWorker,
			IOptimizationPreferences optimizationPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var selectedPeriod = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
			var selectedAgents = _schedulerStateHolder().AllPermittedPersons.Where(x => @event.AgentsToSchedule.Contains(x.Id.Value));
			_scheduleExecutor.Execute(schedulingCallback, _schedulingOptionsProvider.Fetch(), backgroundWorker, selectedAgents, selectedPeriod, optimizationPreferences, @event.RunWeeklyRestSolver, dayOffOptimizationPreferenceProvider);
		}
	}
}