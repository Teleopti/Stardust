using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Historical;

namespace Teleopti.Wfm.Adherence.States.Events
{
	[JsonObject(Id = "ArrivedLateForWork")]
	public class PersonArrivedLateForWorkEvent : IRtaStoredEvent, IEvent, ISolidProof
	{
		public Guid PersonId { get; set; }
		public DateOnly? BelongsToDate { get; set; }

		public DateTime Timestamp { get; set; }

		public DateTime ShiftStart { get; set; }

		public string StateName { get; set; }
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