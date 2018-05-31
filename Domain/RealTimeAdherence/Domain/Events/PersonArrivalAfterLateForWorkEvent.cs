using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events
{
	public class PersonArrivalAfterLateForWorkEvent : IRtaStoredEvent, IEvent, ISolidProof
	{
		public Guid PersonId { get; set; }

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
				StartTime = Timestamp,
				EndTime = Timestamp
			};
	}
}