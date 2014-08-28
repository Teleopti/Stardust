using System;

namespace Teleopti.Interfaces.Domain
{
	public class TeleoptiDiagnosticsInformation : ITeleoptiDiagnosticsInformation
	{
		public DateTime HandledAt { get; set; }
		public DateTime SentAt { get; set; }
		public int MillisecondsDifference { get; set; }
	}
}