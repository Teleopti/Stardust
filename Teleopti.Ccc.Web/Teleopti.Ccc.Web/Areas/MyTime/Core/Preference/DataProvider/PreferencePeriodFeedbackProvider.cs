using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferencePeriodFeedbackProvider : IPreferencePeriodFeedbackProvider
	{
		private readonly IVirtualSchedulePeriodProvider _virtualSchedulePeriodProvider;
		private readonly ISchedulePeriodTargetDayOffCalculator _schedulePeriodTargetDayOffCalculator;

		public PreferencePeriodFeedbackProvider(IVirtualSchedulePeriodProvider virtualSchedulePeriodProvider, ISchedulePeriodTargetDayOffCalculator schedulePeriodTargetDayOffCalculator)
		{
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_schedulePeriodTargetDayOffCalculator = schedulePeriodTargetDayOffCalculator;
		}

		public DaysOffViewModel ShouldHaveDaysOff(DateOnly date)
		{
			var virtualSchedulePeriod = _virtualSchedulePeriodProvider.VirtualSchedulePeriodForDate(date);
			var result = _schedulePeriodTargetDayOffCalculator.TargetDaysOff(virtualSchedulePeriod);
			return new DaysOffViewModel
			{
				Lower = result.Minimum,
				Upper = result.Maximum
			};
		}
	}

	public class DaysOffViewModel
	{
		public int Lower { get; set; }
		public int Upper { get; set; }
	}
}