using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public interface IAgentStudentAvailabilityView
	{
		void Update(TimeSpan? startTime, TimeSpan? endTime);
	}
}
