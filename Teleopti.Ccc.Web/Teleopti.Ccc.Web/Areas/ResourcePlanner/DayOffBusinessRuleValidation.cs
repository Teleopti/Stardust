using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class DayOffBusinessRuleValidation
	{
		public bool Validate(IScheduleRange scheduleRange)
		{
			if (!(scheduleRange.CalculatedTargetScheduleDaysOff.HasValue && scheduleRange.CalculatedScheduleDaysOff.HasValue))
				return false;
			return (scheduleRange.CalculatedTargetScheduleDaysOff == scheduleRange.CalculatedScheduleDaysOff);
		}
	}
}