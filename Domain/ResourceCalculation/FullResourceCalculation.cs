using System;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class FullResourceCalculation
	{
		private readonly IResourceCalculation _cascadingResourceCalculation;
		private readonly Func<ISchedulerStateHolder> _stateHolder;

		public FullResourceCalculation(IResourceCalculation cascadingResourceCalculation, Func<ISchedulerStateHolder> stateHolder)
		{
			_cascadingResourceCalculation = cascadingResourceCalculation;
			_stateHolder = stateHolder;
		}

		public void Execute()
		{
			var stateHolder = _stateHolder();
			_cascadingResourceCalculation.ResourceCalculate(stateHolder.RequestedPeriod.DateOnlyPeriod, stateHolder.SchedulingResultState.ToResourceOptimizationData(stateHolder.ConsiderShortBreaks, false));
		}
	}
}