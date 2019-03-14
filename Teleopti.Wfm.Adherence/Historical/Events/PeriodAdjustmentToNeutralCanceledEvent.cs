using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Historical.Events
{
	public class PeriodAdjustmentToNeutralCanceledEvent : IEvent, IRtaStoredEvent
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public QueryData QueryData() =>
			new QueryData
			{
				StartTime = StartTime,
				EndTime = EndTime
			};
	}
}