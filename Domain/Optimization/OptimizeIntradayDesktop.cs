using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Cascading;
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
		private readonly ShovelResources _shovelResources;

		public OptimizeIntradayIslandsDesktop(IntradayOptimizationCommandHandler intradayOptimizationCommandHandler, 
																		DesktopOptimizationContext desktopOptimizationContext,
																		Func<ISchedulerStateHolder> currentSchedulerStateHolder,
																		ShovelResources shovelResources)
		{
			_intradayOptimizationCommandHandler = intradayOptimizationCommandHandler;
			_desktopOptimizationContext = desktopOptimizationContext;
			_currentSchedulerStateHolder = currentSchedulerStateHolder;
			_shovelResources = shovelResources;
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
			_shovelResources.Execute(selectedPeriod);
		}	
	}
}
