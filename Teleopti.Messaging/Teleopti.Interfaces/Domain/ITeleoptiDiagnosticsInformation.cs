using System;

namespace Teleopti.Interfaces.Domain
{
	public interface ITeleoptiDiagnosticsInformation
	{
		DateTime HandledAt { get; set; }
		DateTime SentAt { get; set; }
		int MillisecondsDifference { get; set; }
	}
}