using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IsNightShift
	{
		private readonly ITimeZoneGuard _timeZoneGuard;

		public IsNightShift(ITimeZoneGuard timeZoneGuard)
		{
			_timeZoneGuard = timeZoneGuard;
		}

		public bool InEndUserTimeZone(IScheduleDay scheduleDay)
		{
			var tz = _timeZoneGuard.CurrentTimeZone();
			var personAssignmentPeriod = scheduleDay.PersonAssignment(true).Period;
			var viewerStartDate = new DateOnly(personAssignmentPeriod.StartDateTimeLocal(tz));
			var viewerEndDate = new DateOnly(personAssignmentPeriod.EndDateTimeLocal(tz).AddMinutes(-1));

			return viewerStartDate != viewerEndDate;
		}
	}
}