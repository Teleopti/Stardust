using System;
using Teleopti.Ccc.Domain.Islands.Legacy;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class IntradayOptimizationContext
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly VirtualSkillContext _virtualSkillContext;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContext;

		public IntradayOptimizationContext(Func<ISchedulerStateHolder> schedulerStateHolder, 
				VirtualSkillContext virtualSkillContext,
				CascadingResourceCalculationContextFactory resourceCalculationContext)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_virtualSkillContext = virtualSkillContext;
			_resourceCalculationContext = resourceCalculationContext;
		}

		public IDisposable Create(DateOnlyPeriod period)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var virtualSkillContext = _virtualSkillContext.Create(period);
			var resourceContext = _resourceCalculationContext.Create(schedulerStateHolder.Schedules, schedulerStateHolder.SchedulingResultState.Skills, true, period);
			return new GenericDisposable(() =>
			{
				resourceContext.Dispose();
				virtualSkillContext.Dispose();
			});
		}
	}
}