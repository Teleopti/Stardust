using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class DayOffBusinessRuleValidation
	{
		public bool Validate(IScheduleRange scheduleRange, DateOnlyPeriod periodToCheck)
		{
			if (!(scheduleRange.CalculatedTargetScheduleDaysOff(periodToCheck).HasValue))
				return false;

			var calculatedTargetScheduleDaysOff = scheduleRange.CalculatedTargetScheduleDaysOff(periodToCheck);
			return calculatedTargetScheduleDaysOff != null && (calculatedTargetScheduleDaysOff.Value == scheduleRange.CalculatedScheduleDaysOffOnPeriod(periodToCheck));
		}
	}
}