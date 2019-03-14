using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Historical.Events
{
	public class PeriodAdjustmentToNeutralCanceledEvent : IEvent
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}
}