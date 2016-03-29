using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IOptimizeIntradayDesktop
	{
		//TODO - remove unneeded params when toggle is gone
		void Optimize(IEnumerable<IScheduleDay> scheduleDays, IOptimizationPreferences optimizerPreferences,
			DateOnlyPeriod selectedPeriod, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			IIntradayOptimizationCallback intradayOptimizationCallback);	
	}

	public class OptimizeIntradayIslandsDesktop : IOptimizeIntradayDesktop
	{
		private readonly IIntradayOptimizationCommandHandler _intradayOptimizationCommandHandler;
		private readonly DesktopOptimizationContext _desktopOptimizationContext;
		private readonly Func<ISchedulerStateHolder> _currentSchedulerStateHolder;

		public OptimizeIntradayIslandsDesktop(IIntradayOptimizationCommandHandler intradayOptimizationCommandHandler, 
																		DesktopOptimizationContext desktopOptimizationContext,
																		Func<ISchedulerStateHolder> currentSchedulerStateHolder)
		{
			_intradayOptimizationCommandHandler = intradayOptimizationCommandHandler;
			_desktopOptimizationContext = desktopOptimizationContext;
			_currentSchedulerStateHolder = currentSchedulerStateHolder;
		}

		public void Optimize(IEnumerable<IScheduleDay> scheduleDays, IOptimizationPreferences optimizerPreferences, DateOnlyPeriod selectedPeriod,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, IIntradayOptimizationCallback intradayOptimizationCallback)
		{
			using (_desktopOptimizationContext.Set(_currentSchedulerStateHolder(), optimizerPreferences, intradayOptimizationCallback))
			{
				_intradayOptimizationCommandHandler.Execute(new IntradayOptimizationCommand
				{
					Period = selectedPeriod,
					AgentsToOptimize = scheduleDays.Select(x=> x.Person).Distinct()
				});
			}
		}	
	}

	public class OptimizeIntradayDesktop : IOptimizeIntradayDesktop
	{
		private readonly IntradayOptimizationContext _intradayOptimizationContext;
		private readonly IntradayOptimizationCallbackContext _intradayOptimizationCallbackContext;
		private readonly IntradayOptimizer2Creator _intradayOptimizer2Creator;
		private readonly IIntradayOptimizerContainer _intradayOptimizerContainer;


		public OptimizeIntradayDesktop(IntradayOptimizationContext intradayOptimizationContext,
										IntradayOptimizationCallbackContext intradayOptimizationCallbackContext,
										IntradayOptimizer2Creator intradayOptimizer2Creator,
										IIntradayOptimizerContainer intradayOptimizerContainer)
		{
			_intradayOptimizationContext = intradayOptimizationContext;
			_intradayOptimizationCallbackContext = intradayOptimizationCallbackContext;
			_intradayOptimizer2Creator = intradayOptimizer2Creator;
			_intradayOptimizerContainer = intradayOptimizerContainer;
		}

		public void Optimize(IEnumerable<IScheduleDay> scheduleDays, IOptimizationPreferences optimizerPreferences,
							DateOnlyPeriod selectedPeriod, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
							IIntradayOptimizationCallback intradayOptimizationCallback)
		{
			var optimizers = _intradayOptimizer2Creator
				.Create(selectedPeriod, scheduleDays, optimizerPreferences, dayOffOptimizationPreferenceProvider);

			using (_intradayOptimizationContext.Create(selectedPeriod))
			{
				using (_intradayOptimizationCallbackContext.Create(intradayOptimizationCallback))
				{
					_intradayOptimizerContainer.Execute(optimizers);
				}
			}
		}
	}
}
