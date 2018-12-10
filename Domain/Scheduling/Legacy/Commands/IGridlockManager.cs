using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IGridlockManager
	{
		bool HasLocks { get; }
		void AddLock(IPerson person, DateOnly dateOnly, LockType lockType);
		void AddLock(IScheduleDay schedulePart, LockType lockType);
		void AddLock(IList<IScheduleDay> schedules, LockType lockType);
		void RemoveLock(IPerson person, DateOnly dateOnly);
		void RemoveLock(IPerson person, DateOnly dateOnly, LockType lockType);
		void RemoveLock(IScheduleDay schedulePart);
		void RemoveLock(IList<IScheduleDay> schedules);

		GridlockDictionary Gridlocks(IPerson person, DateOnly dateOnly);
		IList<IScheduleDay> UnlockedDays(IList<IScheduleDay> scheduleDays);
		GridlockDictionary Gridlocks(IScheduleDay schedulePart);

		void Clear();
		void ClearWriteProtected();

		IEnumerable<LockInfo> LockInfos();
	}
}