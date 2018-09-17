using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Domain.Events
{
	public class PeriodApprovedAsInAdherenceEvent : IRtaStoredEvent, IEvent
	{
		public Guid PersonId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public QueryData QueryData() =>
			new QueryData
			{
				PersonId = PersonId,
				StartTime = StartTime,
				EndTime = EndTime
			};
	}
}