using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferencePeriodFeedbackProvider : IPreferencePeriodFeedbackProvider
	{
		private readonly IVirtualSchedulePeriodProvider _virtualSchedulePeriodProvider;
		private readonly ISchedulePeriodTargetDayOffCalculator _schedulePeriodTargetDayOffCalculator;
		private readonly ISchedulePeriodPossibleResultDayOffCalculator _schedulePeriodPossibleResultDayOffCalculator;

		public PreferencePeriodFeedbackProvider(IVirtualSchedulePeriodProvider virtualSchedulePeriodProvider, ISchedulePeriodTargetDayOffCalculator schedulePeriodTargetDayOffCalculator, ISchedulePeriodPossibleResultDayOffCalculator schedulePeriodPossibleResultDayOffCalculator)
		{
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_schedulePeriodTargetDayOffCalculator = schedulePeriodTargetDayOffCalculator;
			_schedulePeriodPossibleResultDayOffCalculator = schedulePeriodPossibleResultDayOffCalculator;
		}

		public MinMax<int> TargetDaysOff(DateOnly date)
		{
			var virtualSchedulePeriod = _virtualSchedulePeriodProvider.VirtualSchedulePeriodForDate(date);
			return _schedulePeriodTargetDayOffCalculator.TargetDaysOff(virtualSchedulePeriod);
		}

		public int PossibleResultDaysOff(DateOnly date)
		{
			return _schedulePeriodPossibleResultDayOffCalculator.PossibleResultDayOff();
		}
	}

	public interface ISchedulePeriodPossibleResultDayOffCalculator
	{
		int PossibleResultDayOff();
	}
}