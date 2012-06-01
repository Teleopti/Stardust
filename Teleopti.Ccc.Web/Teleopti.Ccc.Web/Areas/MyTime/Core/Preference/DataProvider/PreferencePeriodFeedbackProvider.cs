using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferencePeriodFeedbackProvider : IPreferencePeriodFeedbackProvider
	{
		private readonly IVirtualSchedulePeriodProvider _virtualSchedulePeriodProvider;
		private readonly ISchedulePeriodTargetDayOffCalculator _schedulePeriodTargetDayOffCalculator;
		private readonly IPeriodScheduledAndRestrictionDaysOff _periodScheduledAndRestrictionDaysOff;
		private readonly IScheduleProvider _scheduleProvider;

		public PreferencePeriodFeedbackProvider(IVirtualSchedulePeriodProvider virtualSchedulePeriodProvider, ISchedulePeriodTargetDayOffCalculator schedulePeriodTargetDayOffCalculator, IPeriodScheduledAndRestrictionDaysOff periodScheduledAndRestrictionDaysOff, IScheduleProvider scheduleProvider)
		{
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_schedulePeriodTargetDayOffCalculator = schedulePeriodTargetDayOffCalculator;
			_periodScheduledAndRestrictionDaysOff = periodScheduledAndRestrictionDaysOff;
			_scheduleProvider = scheduleProvider;
		}

		public MinMax<int> TargetDaysOff(DateOnly date)
		{
			var virtualSchedulePeriod = _virtualSchedulePeriodProvider.VirtualSchedulePeriodForDate(date);
			return _schedulePeriodTargetDayOffCalculator.TargetDaysOff(virtualSchedulePeriod);
		}

		public int PossibleResultDaysOff(DateOnly date)
		{
			var period = _virtualSchedulePeriodProvider.GetCurrentOrNextVirtualPeriodForDate(date);
			var scheduleDays = _scheduleProvider.GetScheduleForPeriod(period);
			return _periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(scheduleDays, true, true, false);
		}
	}
}