using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Wfm.Adherence.Domain.Events
{
	[JsonObject(Id = "ShiftEnd")]
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
				BelongsToDate = BelongsToDate,
				StartTime = ShiftEndTime,
				EndTime = ShiftEndTime
			};
	}
}