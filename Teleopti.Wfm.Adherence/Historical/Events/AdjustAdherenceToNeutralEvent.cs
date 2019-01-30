using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Historical.Events
{
	public class AdjustAdherenceToNeutralEvent : IEvent
	{
		public DateTime StartTime;
		public DateTime EndTime;
	}
}