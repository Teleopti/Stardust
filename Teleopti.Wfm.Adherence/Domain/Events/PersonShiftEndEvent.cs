using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Adherence.Domain.Events
{
	public class PersonShiftEndEvent : IEvent, IRtaStoredEvent
	{
		public Guid PersonId { get; set; }
		public DateOnly? BelongsToDate { get; set; }
		public DateTime ShiftStartTime { get; set; }
		public DateTime ShiftEndTime { get; set; }

		public QueryData QueryData() =>
			new QueryData
			{
				PersonId = PersonId,
				StartTime = ShiftEndTime,
				EndTime = ShiftEndTime
			};
	}
}