using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ITeleoptiDiagnosticsInformation
	{
		DateTime HandledAt { get; set; }
		DateTime SentAt { get; set; }
		int MillisecondsDifference { get; set; }
	}
}