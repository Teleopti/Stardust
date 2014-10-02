using System;

namespace Teleopti.Ccc.Rta.TestApplication
{
	public interface ITeleoptiRtaState
	{
		string LogOn { get; set; }
		string StateCode { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "InState")]
		TimeSpan TimeInState { get; set; }
		DateTime Timestamp { get; set; }
		DateTime BatchId { get; set; }
		bool IsSnapshot { get; set; }
	}

	public class TeleoptiRtaState : ITeleoptiRtaState
	{
		public string LogOn { get; set; }
		public string StateCode { get; set; }
		public TimeSpan TimeInState { get; set; }
		public DateTime Timestamp { get; set; }
		public DateTime BatchId { get; set; }
		public bool IsSnapshot { get; set; }
	}
}