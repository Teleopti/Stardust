using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public interface IAgentOvertimeAvailabilityView
	{
		void Update(TimeSpan? startTime, TimeSpan? endTime);
	}
}