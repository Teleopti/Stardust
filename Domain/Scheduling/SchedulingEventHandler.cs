using System;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingEventHandler
	{
		private readonly IScheduling _scheduling;
		private readonly RuleSetBagsOfGroupOfPeopleCanHaveShortBreak _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

		public SchedulingEventHandler(IScheduling scheduling,
			RuleSetBagsOfGroupOfPeopleCanHaveShortBreak ruleSetBagsOfGroupOfPeopleCanHaveShortBreak,
			Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_scheduling = scheduling;
			_ruleSetBagsOfGroupOfPeopleCanHaveShortBreak = ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;
			_schedulerStateHolder = schedulerStateHolder;
		}

		public void HandleEvent(SchedulingWasOrdered @event,
			//remove these later
			SchedulingOptions schedulingOptions,
			ISchedulingCallback schedulingCallback,
			ISchedulingProgress backgroundWorker,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var period = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
			var agentsToSchedule = _schedulerStateHolder().AllPermittedPersons.Where(x => @event.AgentsToOptimize.Contains(x.Id.Value));

			schedulingOptions.ConsiderShortBreaks = _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak.CanHaveShortBreak(agentsToSchedule, period);
			_scheduling.Execute(schedulingCallback, schedulingOptions, backgroundWorker, agentsToSchedule, period, dayOffOptimizationPreferenceProvider);
		}
	}
}