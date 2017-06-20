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

		public SchedulingEventHandler(Func<ISchedulerStateHolder> schedulerStateHolder, IScheduleExecutor scheduleExecutor)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleExecutor = scheduleExecutor;
		}

		public void HandleEvent(SchedulingWasOrdered @event,
			//remove these later
			SchedulingOptions schedulingOptions,
			ISchedulingCallback schedulingCallback,
			ISchedulingProgress backgroundWorker,
			IOptimizationPreferences optimizationPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			bool runWeeklyRestSolver)
		{
			var selectedPeriod = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
			var selectedAgents = _schedulerStateHolder().AllPermittedPersons.Where(x => @event.AgentsToSchedule.Contains(x.Id.Value));
			_scheduleExecutor.Execute(schedulingCallback, schedulingOptions, backgroundWorker, selectedAgents, selectedPeriod, optimizationPreferences, runWeeklyRestSolver, dayOffOptimizationPreferenceProvider);
		}
	}
}