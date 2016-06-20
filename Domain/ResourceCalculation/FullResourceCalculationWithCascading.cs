using System;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class FullResourceCalculationWithCascading : IFullResourceCalculation
	{
		private readonly CascadingResourceCalculation _cascadingResourceCalculation;
		private readonly Func<ISchedulerStateHolder> _stateHolder;

		public FullResourceCalculationWithCascading(CascadingResourceCalculation cascadingResourceCalculation, Func<ISchedulerStateHolder> stateHolder)
		{
			_cascadingResourceCalculation = cascadingResourceCalculation;
			_stateHolder = stateHolder;
		}

		public void Execute()
		{
			var stateHolder = _stateHolder();
			_cascadingResourceCalculation.ForAll(stateHolder.RequestedPeriod.DateOnlyPeriod, stateHolder.SchedulingResultState.ToResourceOptimizationData(stateHolder.ConsiderShortBreaks, false));
		}
	}
}