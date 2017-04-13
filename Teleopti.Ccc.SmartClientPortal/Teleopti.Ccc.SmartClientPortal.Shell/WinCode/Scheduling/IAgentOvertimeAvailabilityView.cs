using System;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IAgentOvertimeAvailabilityView
	{
		void Update(TimeSpan? startTime, TimeSpan? endTime);
	}
}