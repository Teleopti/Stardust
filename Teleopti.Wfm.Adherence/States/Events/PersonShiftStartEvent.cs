using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Historical;

namespace Teleopti.Wfm.Adherence.States.Events
{
	[JsonObject(Id = "ShiftStart")]
	public class PersonShiftStartEvent : IEvent, IRtaStoredEvent
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
				StartTime = ShiftStartTime,
				EndTime = ShiftStartTime
			};
	}
}