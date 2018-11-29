using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Wfm.Adherence.Domain.Events
{
	[JsonObject(Id = "StateChanged")]
	public class PersonStateChangedEvent : IRtaStoredEvent, IEvent, ISolidProof
	{
		public Guid PersonId { get; set; }
		public DateOnly? BelongsToDate { get; set; }
		public DateTime Timestamp { get; set; }

		public string StateName { get; set; }
		public Guid? StateGroupId { get; set; }

		public string ActivityName { get; set; }
		public int? ActivityColor { get; set; }

		public string RuleName { get; set; }
		public int? RuleColor { get; set; }

		public EventAdherence? Adherence { get; set; }

		public QueryData QueryData() =>
			new QueryData
			{
				PersonId = PersonId,
				BelongsToDate = BelongsToDate,
				StartTime = Timestamp,
				EndTime = Timestamp
			};
	}
}