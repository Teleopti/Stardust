using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class DoFullResourceOptimizationOneTime
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IResourceCalculation _resourceCalculation;

		public DoFullResourceOptimizationOneTime(Func<ISchedulerStateHolder> schedulerStateHolder, IResourceCalculation resourceCalculation)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_resourceCalculation = resourceCalculation;
		}

		public void ExecuteIfNecessary()
		{
			var stateHolder = _schedulerStateHolder();
			if (ResourceCalculationContext.PrimarySkillMode() || !stateHolder.SchedulingResultState.GuessResourceCalculationHasBeenMade())
			{
				_resourceCalculation.ResourceCalculate(stateHolder.RequestedPeriod.DateOnlyPeriod, new ResourceCalculationData(stateHolder.SchedulingResultState, false, false));
			}
		}
	}
}