using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class DayOffBusinessRuleValidation
	{
		public bool Validate(IScheduleRange scheduleRange, DateOnlyPeriod periodToCheck)
		{
			if (!(scheduleRange.CalculatedTargetScheduleDaysOff(periodToCheck).HasValue))
				return false;
			return (scheduleRange.CalculatedTargetScheduleDaysOff(periodToCheck) == scheduleRange.CalculatedScheduleDaysOff);
		}
	}
}