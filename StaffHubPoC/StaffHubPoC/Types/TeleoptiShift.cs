using System;

namespace StaffHubPoC.Types
{
	public class TeleoptiShift
	{
		public string Email { get; set; }
		public string Label { get; set; }
		public DateTime BelongsToDate { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}
}
