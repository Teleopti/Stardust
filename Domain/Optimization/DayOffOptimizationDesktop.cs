using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationDesktop
	{
		private readonly IDayOffOptimizationCommandHandler _dayOffOptimizationCommandHandler;

		public DayOffOptimizationDesktop(IDayOffOptimizationCommandHandler dayOffOptimizationCommandHandler)
		{
			_dayOffOptimizationCommandHandler = dayOffOptimizationCommandHandler;
		}

		public void Execute(DateOnlyPeriod selectedPeriod, 
			IEnumerable<IPerson> selectedAgents, 
			ISchedulingProgress backgroundWorker,
			IOptimizationPreferences optimizationPreferences, 
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
		{
			var blockPreferenceProvider = new FixedBlockPreferenceProvider(optimizationPreferences.Extra);

			_dayOffOptimizationCommandHandler.Execute(new DayOffOptimizationCommand
				{
					Period = selectedPeriod,
					AgentsToOptimize = selectedAgents,
					RunWeeklyRestSolver = false
				},
				optimizationPreferences,
				dayOffOptimizationPreferenceProvider, 
				blockPreferenceProvider, 
				backgroundWorker, 
				resourceOptimizerPersonOptimized);
		}
	}
}