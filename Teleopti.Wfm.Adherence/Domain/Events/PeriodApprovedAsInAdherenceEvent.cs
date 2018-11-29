using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Wfm.Adherence.Domain.Events
{
	[JsonObject(Id = "ApprovedAsInAdherence")]
	public class PeriodApprovedAsInAdherenceEvent : IRtaStoredEvent, IEvent
	{
		public Guid PersonId { get; set; }
		public DateOnly? BelongsToDate { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public QueryData QueryData() =>
			new QueryData
			{
				PersonId = PersonId,
				BelongsToDate = BelongsToDate,
				StartTime = StartTime,
				EndTime = EndTime
			};
	}
}