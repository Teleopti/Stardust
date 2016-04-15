using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class DayOffOptimizationDesktopClassic : IDayOffOptimizationDesktop
	{
		private readonly ClassicDaysOffOptimizationCommand _classicDaysOffOptimizationCommand;

		public DayOffOptimizationDesktopClassic(ClassicDaysOffOptimizationCommand classicDaysOffOptimizationCommand)
		{
			_classicDaysOffOptimizationCommand = classicDaysOffOptimizationCommand;
		}

		public void Execute(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization, DateOnlyPeriod selectedPeriod,
			ISchedulingProgress backgroundWorker, IOptimizationPreferences optimizationPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			_classicDaysOffOptimizationCommand.Execute(matrixOriginalStateContainerListForDayOffOptimization, selectedPeriod,
				optimizationPreferences, backgroundWorker, dayOffOptimizationPreferenceProvider);
		}
	}
}