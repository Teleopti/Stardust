using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Historical;
using Teleopti.Wfm.Adherence.Historical.Events;

namespace Teleopti.Wfm.Adherence.States.Events
{
	public class PersonAdherenceDayStartEvent : IEvent, ISolidProof, IRtaStoredEvent, IRtaStoredEventForPerson
	{
		public Guid PersonId { get; set; }
		public DateTime Timestamp { get; set; }
		public DateOnly? BelongsToDate { get; set; }

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