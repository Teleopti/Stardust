using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
	internal class SchedulingAndOptimizeArgument
	{
		public IList<IScheduleDay> SelectedScheduleDays { get; private set; }
		public OptimizationMethod OptimizationMethod { get; set; }
		public IDaysOffPreferences DaysOffPreferences { get; set; }
		public IOvertimePreferences OvertimePreferences { get; set; }

		public SchedulingAndOptimizeArgument(IList<IScheduleDay> selectedScheduleDays)
		{
			SelectedScheduleDays = selectedScheduleDays;
		}
	}
}
