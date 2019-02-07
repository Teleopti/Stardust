using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Historical.Events
{
	[JsonObject(Id = "AdjustedToNeutralAdherence")]
	public class PeriodAdjustedToNeutralEvent : IEvent, IRtaStoredEvent
	{
		public DateTime StartTime;
		public DateTime EndTime;
		public QueryData QueryData() =>
			new QueryData
			{
				StartTime = StartTime,
				EndTime = EndTime
			};
	}
}