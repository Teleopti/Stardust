using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		public DayOffOptimizationDesktop(IDayOffOptimizationCommandHandler dayOffOptimizationCommandHandler,
			DesktopOptimizationContext desktopOptimizationContext,
			Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_dayOffOptimizationCommandHandler = dayOffOptimizationCommandHandler;
			_desktopOptimizationContext = desktopOptimizationContext;
			_schedulerStateHolder = schedulerStateHolder;
		}

		public void Execute(DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedAgents, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized, IOptimizationCallback optimizationCallback)
		{
			var command = new DayOffOptimizationCommand
			{
				Period = selectedPeriod,
				AgentsToOptimize = selectedAgents,
				RunWeeklyRestSolver = false
			};
			using (_desktopOptimizationContext.Set(command, _schedulerStateHolder(), optimizationPreferences, dayOffOptimizationPreferenceProvider, optimizationCallback))
			{
				_dayOffOptimizationCommandHandler.Execute(command,
					resourceOptimizerPersonOptimized);
			}
		}
	}
}