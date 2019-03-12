using System;
using System.Diagnostics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	[DebuggerDisplay("{StartDate} -> {EndDate} [{DaysInRange}]: {PersonId}")]
	public class PersonDayProjectionChanged
	{
		public PersonDayProjectionChanged(Guid personId, DateOnly startDate, DateOnly endDate)
		{
			PersonId = personId;
			StartDate = startDate;
			EndDate = endDate;
			DaysInRange = (endDate.Date - startDate.Date).Days + 1;
		}
		public Guid PersonId { get; set; }
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public int DaysInRange { get; set; }
	}
}
