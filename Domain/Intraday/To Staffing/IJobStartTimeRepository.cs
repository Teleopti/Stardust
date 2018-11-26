using System;

namespace Teleopti.Ccc.Domain.Staffing
{
	public interface IJobStartTimeRepository
	{
		bool CheckAndUpdate(int thresholdMinutes, Guid bu);
		void UpdateLockTimestamp(Guid bu);
		void ResetLockTimestamp(Guid bu);
		void RemoveLock(Guid bu);
	  }
}