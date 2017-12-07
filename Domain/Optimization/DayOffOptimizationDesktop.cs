using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationDesktop
	{
		private readonly DayOffOptimization _dayOffOptimization;

		public DayOffOptimizationDesktop(DayOffOptimization dayOffOptimization)
		{
			_dayOffOptimization = dayOffOptimization;
		}

		public void Execute(DateOnlyPeriod selectedPeriod, 
			IEnumerable<IPerson> selectedAgents, 
			ISchedulingProgress backgroundWorker,
			IOptimizationPreferences optimizationPreferences, 
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
		{
			var blockPreferenceProvider = new FixedBlockPreferenceProvider(optimizationPreferences.Extra);

			_dayOffOptimization.Execute(selectedPeriod, selectedAgents, optimizationPreferences, 
					dayOffOptimizationPreferenceProvider, blockPreferenceProvider, backgroundWorker, false, resourceOptimizerPersonOptimized);
		}
	}
}