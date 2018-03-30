using System;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class OvertimeDefaultStartTimeResult
	{
		public bool IsShiftStartTime { get; set; }
		public bool IsShiftEndTime { get; set; }
		public DateTime DefaultStartTime { get; set; }
		public string DefaultStartTimeString { get; set; }
	}
}