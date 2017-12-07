using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.ClassicLegacy;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationDesktopTeamBlock
	{
		private readonly DayOffOptimization _dayOffOptimization;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;

		public DayOffOptimizationDesktopTeamBlock(DayOffOptimization dayOffOptimization,
								Func<ISchedulerStateHolder> schedulerStateHolder,
								CascadingResourceCalculationContextFactory resourceCalculationContextFactory)
		{
			_dayOffOptimization = dayOffOptimization;
			_schedulerStateHolder = schedulerStateHolder;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
		}

		public void Execute(DateOnlyPeriod selectedPeriod, 
			IEnumerable<IPerson> selectedAgents, 
			ISchedulingProgress backgroundWorker,
			IOptimizationPreferences optimizationPreferences, 
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
		{
			var stateHolder = _schedulerStateHolder();
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
			var blockPreferenceProvider = new FixedBlockPreferenceProvider(optimizationPreferences.Extra);

			using (_resourceCalculationContextFactory.Create(stateHolder.SchedulingResultState, true, selectedPeriod.Inflate(1)))
			{
				_dayOffOptimization.Execute(selectedPeriod, selectedAgents, optimizationPreferences, schedulingOptions, 
					dayOffOptimizationPreferenceProvider, blockPreferenceProvider, backgroundWorker, false, resourceOptimizerPersonOptimized);
			}
		}
	}
}