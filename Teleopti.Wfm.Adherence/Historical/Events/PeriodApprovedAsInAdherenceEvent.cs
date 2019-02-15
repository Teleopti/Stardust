using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Historical.Events
{
	public class PeriodApprovedAsInAdherenceEvent : IRtaStoredEvent, ISynchronizationInfo, IEvent
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
		
		public SynchronizationInfo SynchronizationInfo() =>
			new SynchronizationInfo
			{
				PersonId = PersonId,
				BelongsToDate = BelongsToDate,
				StartTime = StartTime
			};
	}
}