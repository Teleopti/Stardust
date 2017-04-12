using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IJobStartTimeRepository
	{
		bool CheckAndUpdate(int thresholdMinutes, Guid bu);
		void UpdateLockTimestamp(Guid bu);
		void ResetLockTimestamp(Guid bu);
	}
}