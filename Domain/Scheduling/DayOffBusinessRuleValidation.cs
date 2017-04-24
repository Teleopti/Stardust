using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class DayOffBusinessRuleValidation
	{
		public bool Validate(IScheduleRange scheduleRange, DateOnlyPeriod periodToCheck)
		{
			var summary = scheduleRange.CalculatedTargetTimeSummary(periodToCheck);
			if (!summary.TargetDaysOff.HasValue)
				return false;

			var scheduledDaysOff = scheduleRange.CalculatedScheduleDaysOffOnPeriod(periodToCheck);
			return summary.TargetDaysOff.Value - summary.NegativeTargetDaysOffTolerance <= scheduledDaysOff &&
				summary.TargetDaysOff.Value + summary.PositiveTargetDaysOffTolerance >= scheduledDaysOff;
		}
	}
}