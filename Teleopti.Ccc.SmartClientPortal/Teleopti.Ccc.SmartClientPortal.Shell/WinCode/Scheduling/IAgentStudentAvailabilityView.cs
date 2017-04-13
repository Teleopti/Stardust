using System;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IAgentStudentAvailabilityView
	{
		void Update(TimeSpan? startTime, TimeSpan? endTime);
	}
}
