using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class DesktopScheduling
	{
		private readonly IScheduleCommand _scheduleCommand;

		public DesktopScheduling(IScheduleCommand scheduleCommand)
		{
			_scheduleCommand = scheduleCommand;
		}

		public void Execute(IOptimizerOriginalPreferences optimizerOriginalPreferences,
			ISchedulingProgress backgroundWorker, IList<IScheduleDay> selectedScheduleDays,
			IOptimizationPreferences optimizationPreferences,
			IDaysOffPreferences dayOffsPreferences)
		{
			_scheduleCommand.Execute(optimizerOriginalPreferences, backgroundWorker, selectedScheduleDays,
				optimizationPreferences, true,
				new FixedDayOffOptimizationPreferenceProvider(dayOffsPreferences));
		}
	}
}