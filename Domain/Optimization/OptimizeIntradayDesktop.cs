using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizeIntradayIslandsDesktop
	{
		private readonly IntradayOptimizationCommandHandler _intradayOptimizationCommandHandler;
		private readonly DesktopOptimizationContext _desktopOptimizationContext;
		private readonly Func<ISchedulerStateHolder> _currentSchedulerStateHolder;
		private readonly IResourceOptimization _resourceOptimizationHelper;

		public OptimizeIntradayIslandsDesktop(IntradayOptimizationCommandHandler intradayOptimizationCommandHandler, 
																		DesktopOptimizationContext desktopOptimizationContext,
																		Func<ISchedulerStateHolder> currentSchedulerStateHolder,
																		IResourceOptimization resourceOptimizationHelper)
		{
			_intradayOptimizationCommandHandler = intradayOptimizationCommandHandler;
			_desktopOptimizationContext = desktopOptimizationContext;
			_currentSchedulerStateHolder = currentSchedulerStateHolder;
			_resourceOptimizationHelper = resourceOptimizationHelper;
		}

		public void Optimize(IEnumerable<IPerson> agents, DateOnlyPeriod selectedPeriod, IOptimizationPreferences optimizerPreferences, IIntradayOptimizationCallback intradayOptimizationCallback)
		{
			var command = new IntradayOptimizationCommand
			{
				Period = selectedPeriod,
				AgentsToOptimize = agents
			};

			using (_desktopOptimizationContext.Set(command, _currentSchedulerStateHolder(), optimizerPreferences, intradayOptimizationCallback))
			{
				_intradayOptimizationCommandHandler.Execute(command);
			}

			//TODO: (probably) enough to shovel resources here (if cascading is turned on) - no need to do res calc
			var resCalcData =_currentSchedulerStateHolder().SchedulingResultState.ToResourceOptimizationData(_currentSchedulerStateHolder().ConsiderShortBreaks, false);
			selectedPeriod.DayCollection().ForEach(x => _resourceOptimizationHelper.ResourceCalculate(x, resCalcData));
		}	
	}
}
