using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizeIntradayDesktop
	{
		private readonly DesktopContextState _desktopContextState;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IntradayOptimizationCommandHandler _intradayOptimizationCommandHandler;

		public OptimizeIntradayDesktop(IntradayOptimizationCommandHandler intradayOptimizationCommandHandler, 
			DesktopContextState desktopContextState,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IResourceCalculation resourceCalculation,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory)
		{
			_intradayOptimizationCommandHandler = intradayOptimizationCommandHandler;
			_desktopContextState = desktopContextState;
			_schedulerStateHolder = schedulerStateHolder;
			_resourceCalculation = resourceCalculation;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
		}
		
		public void Optimize(IEnumerable<IPerson> agents, DateOnlyPeriod selectedPeriod, IOptimizationPreferences optimizerPreferences, IOptimizationCallback optimizationCallback)
		{
			var stateHolder = _schedulerStateHolder();
			var command = new IntradayOptimizationCommand
			{
				Period = selectedPeriod,
				AgentsToOptimize = agents
			};

			using (_desktopContextState.SetForOptimization(command, stateHolder, optimizerPreferences, null, optimizationCallback))
			{
				_intradayOptimizationCommandHandler.Execute(command);
			}

			using (_resourceCalculationContextFactory.Create(_schedulerStateHolder().SchedulingResultState, false, selectedPeriod.Inflate(1)))
			{
				_resourceCalculation.ResourceCalculate(selectedPeriod, new ResourceCalculationData(stateHolder.SchedulingResultState, stateHolder.ConsiderShortBreaks, false));				
			}
		}	
	}
}
