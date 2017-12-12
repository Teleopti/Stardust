using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizeIntradayDesktop : IOptimizeIntradayDesktop
	{
		private readonly DesktopOptimizationContext _desktopOptimizationContext;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IntradayOptimizationCommandHandler _intradayOptimizationCommandHandler;

		public OptimizeIntradayDesktop(IntradayOptimizationCommandHandler intradayOptimizationCommandHandler, 
			DesktopOptimizationContext desktopOptimizationContext,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IResourceCalculation resourceCalculation,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory)
		{
			_intradayOptimizationCommandHandler = intradayOptimizationCommandHandler;
			_desktopOptimizationContext = desktopOptimizationContext;
			_schedulerStateHolder = schedulerStateHolder;
			_resourceCalculation = resourceCalculation;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
		}
		
		public void Optimize(IEnumerable<IPerson> agents, DateOnlyPeriod selectedPeriod, IOptimizationPreferences optimizerPreferences, IIntradayOptimizationCallback intradayOptimizationCallback)
		{
			var stateHolder = _schedulerStateHolder();
			var command = new IntradayOptimizationCommand
			{
				Period = selectedPeriod,
				AgentsToOptimize = agents,
				RunAsynchronously = false
			};

			using (_desktopOptimizationContext.Set(command, stateHolder, optimizerPreferences, null, intradayOptimizationCallback))
			{
				_intradayOptimizationCommandHandler.Execute(command);
			}

			using (_resourceCalculationContextFactory.Create(_schedulerStateHolder().SchedulingResultState, false, selectedPeriod.Inflate(1)))
			{
				_resourceCalculation.ResourceCalculate(selectedPeriod, new ResourceCalculationData(stateHolder.SchedulingResultState, stateHolder.ConsiderShortBreaks, false));				
			}
		}	
	}
	
	
	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680)]
	public interface IOptimizeIntradayDesktop
	{
		void Optimize(IEnumerable<IPerson> agents, DateOnlyPeriod selectedPeriod, IOptimizationPreferences optimizerPreferences, IIntradayOptimizationCallback intradayOptimizationCallback);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680)]
	public class OptimizeIntradayIslandsDesktop : IOptimizeIntradayDesktop
	{
		private readonly IntradayOptimizationCommandHandler _intradayOptimizationCommandHandler;
		private readonly DesktopOptimizationContext _desktopOptimizationContext;
		private readonly Func<ISchedulerStateHolder> _currentSchedulerStateHolder;
		private readonly IResourceCalculation _resourceOptimizationHelper;

		public OptimizeIntradayIslandsDesktop(IntradayOptimizationCommandHandler intradayOptimizationCommandHandler, 
																		DesktopOptimizationContext desktopOptimizationContext,
																		Func<ISchedulerStateHolder> currentSchedulerStateHolder,
																		IResourceCalculation resourceOptimizationHelper)
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
				AgentsToOptimize = agents,
				RunAsynchronously = false
			};

			using (_desktopOptimizationContext.Set(command, _currentSchedulerStateHolder(), optimizerPreferences, null, intradayOptimizationCallback))
			{
				_intradayOptimizationCommandHandler.Execute(command);
			}

			//TODO: (probably) enough to shovel resources here (if cascading is turned on) - no need to do res calc
			var resCalcData =_currentSchedulerStateHolder().SchedulingResultState.ToResourceOptimizationData(_currentSchedulerStateHolder().ConsiderShortBreaks, false);
			selectedPeriod.DayCollection().ForEach(x => _resourceOptimizationHelper.ResourceCalculate(x, resCalcData));
		}	
	}
}
