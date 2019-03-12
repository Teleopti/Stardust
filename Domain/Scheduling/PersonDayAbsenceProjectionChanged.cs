using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class PersonDayAbsenceProjectionChanged
	{
		public PersonDayAbsenceProjectionChanged() { }

		public Guid PersonId { get; set; }
		public DateTimePeriod Period { get; set; }

		public PersonDayProjectionChanged Convert(TimeZoneInfo timeZone)
		{
			var timeZonePeriod = Period.ToDateOnlyPeriod(timeZone);
			return new PersonDayProjectionChanged(PersonId, timeZonePeriod.StartDate, timeZonePeriod.EndDate);
		}
	}
}
