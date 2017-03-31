using System;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	//REMOVE!!!!
	//Using this will lead to bugs! (see eg #43743)
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
			if (!stateHolder.SchedulingResultState.GuessResourceCalculationHasBeenMade())
			{
				_resourceCalculation.ResourceCalculate(stateHolder.RequestedPeriod.DateOnlyPeriod, new ResourceCalculationData(stateHolder.SchedulingResultState, false, false));
			}
		}
	}
}