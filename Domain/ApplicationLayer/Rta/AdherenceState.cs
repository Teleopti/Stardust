using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AdherenceState
	{
		public DateTime Timestamp { get; set; }
		public Adherence Adherence { get; set; }
		public bool ShiftEnded { get; set; }

	}
}