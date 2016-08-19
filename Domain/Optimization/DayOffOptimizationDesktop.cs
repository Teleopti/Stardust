using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public abstract class DayOffOptimizationDesktop : IDayOffOptimizationDesktop
	{
		private void before()
		{
		}

		public void Execute(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization, DateOnlyPeriod selectedPeriod,
			ISchedulingProgress backgroundWorker, IOptimizationPreferences optimizationPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			before();
			Optimize(matrixOriginalStateContainerListForDayOffOptimization, selectedPeriod, backgroundWorker, optimizationPreferences, dayOffOptimizationPreferenceProvider);
			after();
		}

		private void after()
		{
		}


		protected abstract void Optimize(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization, DateOnlyPeriod selectedPeriod, ISchedulingProgress backgroundWorker, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}
}