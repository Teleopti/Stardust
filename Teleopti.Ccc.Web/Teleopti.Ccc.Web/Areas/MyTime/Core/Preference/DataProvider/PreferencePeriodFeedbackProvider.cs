using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferencePeriodFeedbackProvider : IPreferencePeriodFeedbackProvider
	{
		private readonly IVirtualSchedulePeriodProvider _virtualSchedulePeriodProvider;
		private readonly ISchedulePeriodTargetDayOffCalculator _schedulePeriodTargetDayOffCalculator;
		private readonly IPeriodScheduledAndRestrictionDaysOff _periodScheduledAndRestrictionDaysOff;
		private readonly ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;
		private readonly IScheduleProvider _scheduleProvider;

		public PreferencePeriodFeedbackProvider(IVirtualSchedulePeriodProvider virtualSchedulePeriodProvider, ISchedulePeriodTargetDayOffCalculator schedulePeriodTargetDayOffCalculator, IPeriodScheduledAndRestrictionDaysOff periodScheduledAndRestrictionDaysOff, ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator, IScheduleProvider scheduleProvider)
		{
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_schedulePeriodTargetDayOffCalculator = schedulePeriodTargetDayOffCalculator;
			_periodScheduledAndRestrictionDaysOff = periodScheduledAndRestrictionDaysOff;
			_schedulePeriodTargetTimeCalculator = schedulePeriodTargetTimeCalculator;
			_scheduleProvider = scheduleProvider;
		}

		public PeriodFeedback PeriodFeedback(DateOnly date)
		{
			var virtualSchedulePeriod = _virtualSchedulePeriodProvider.VirtualSchedulePeriodForDate(date);
			var scheduleDays = _scheduleProvider.GetScheduleForPeriod(virtualSchedulePeriod.DateOnlyPeriod);

			var targetDaysOff = _schedulePeriodTargetDayOffCalculator.TargetDaysOff(virtualSchedulePeriod);
			var possibleResultDaysOff = _periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(scheduleDays, true, true, false);
			var targetTime = _schedulePeriodTargetTimeCalculator.TargetTimeWithTolerance(virtualSchedulePeriod, scheduleDays);

			return new PeriodFeedback
			    {
					TargetDaysOff = targetDaysOff,
					PossibleResultDaysOff = possibleResultDaysOff,
					TargetTime = targetTime
			    };
		}
	}

	public class PeriodFeedback
	{
		public MinMax<int> TargetDaysOff { get; set; }
		public int PossibleResultDaysOff { get; set; }
		public TimePeriod TargetTime { get; set; }
	}
}