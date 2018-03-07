using System;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events
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