using System;

namespace Teleopti.Ccc.Rta.Server
{
	public struct RtaAgentState
	{
		public string AuthenticationKey { get; set; }
		public string UserCode { get; set; }
		public string StateCode { get; set; }
		public string StateDescription { get; set; }
		public bool IsLoggedOn { get; set; }
		public DateTime Timestamp { get; set; }
		public string PlatformTypeId { get; set; }
		public string SourceId { get; set; }
		public DateTime BatchId { get; set; }
		public bool IsSnapshot { get; set; }
	}
}