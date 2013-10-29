using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IUnvalidatedScheduleRangeUpdate
	{
		void SolveConflictBecauseOfExternalInsert(IScheduleData databaseVersion, bool discardMyChanges);
		void SolveConflictBecauseOfExternalUpdate(IScheduleData databaseVersion, bool discardMyChanges);
		IPersistableScheduleData SolveConflictBecauseOfExternalDeletion(Guid id, bool discardMyChanges);
	}
}