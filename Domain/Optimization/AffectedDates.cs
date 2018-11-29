using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class AffectedDates
	{
		private readonly ITimeZoneGuard _timeZoneGuard;

		public AffectedDates(ITimeZoneGuard timeZoneGuard)
		{
			_timeZoneGuard = timeZoneGuard;
		}

		public IEnumerable<DateOnly> For(IScheduleDay scheduleDay)
		{
			var tz = _timeZoneGuard.CurrentTimeZone();
			var personAssignmentPeriod = scheduleDay.PersonAssignment(true).Period;
			var viewerStartDate = new DateOnly(personAssignmentPeriod.StartDateTimeLocal(tz));
			var viewerEndDate = new DateOnly(personAssignmentPeriod.EndDateTimeLocal(tz).AddMinutes(-1));

			var ret = new List<DateOnly> {viewerStartDate};
			if (viewerStartDate != viewerEndDate)
			{
				ret.Add(viewerEndDate);
			}
			return ret;
		}
	}
}