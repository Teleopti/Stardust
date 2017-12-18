using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationDesktop
	{
		private readonly IDayOffOptimizationCommandHandler _dayOffOptimizationCommandHandler;
		private readonly DesktopOptimizationContext _desktopOptimizationContext;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;

		public DayOffOptimizationDesktop(IDayOffOptimizationCommandHandler dayOffOptimizationCommandHandler,
			DesktopOptimizationContext desktopOptimizationContext,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IResourceCalculation resourceCalculation,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory)
		{
			_dayOffOptimizationCommandHandler = dayOffOptimizationCommandHandler;
			_desktopOptimizationContext = desktopOptimizationContext;
			_schedulerStateHolder = schedulerStateHolder;
			_resourceCalculation = resourceCalculation;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
		}

		public void Execute(DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedAgents, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized, IOptimizationCallback optimizationCallback)
		{
			var stateHolder = _schedulerStateHolder();
			var command = new DayOffOptimizationCommand
			{
				Period = selectedPeriod,
				AgentsToOptimize = selectedAgents,
				RunWeeklyRestSolver = false
			};
			using (_desktopOptimizationContext.Set(command, stateHolder, optimizationPreferences, dayOffOptimizationPreferenceProvider, optimizationCallback))
			{
				_dayOffOptimizationCommandHandler.Execute(command,
					resourceOptimizerPersonOptimized);
			}
			using (_resourceCalculationContextFactory.Create(stateHolder.SchedulingResultState, false, selectedPeriod))
			{
				_resourceCalculation.ResourceCalculate(selectedPeriod, new ResourceCalculationData(stateHolder.SchedulingResultState, false, false));
			}
		}
	}
}