using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IUnvalidatedScheduleRangeUpdate
	{
		void SolveConflictBecauseOfExternalInsert(IScheduleData databaseVersion, bool discardMyChanges);
		void SolveConflictBecauseOfExternalUpdate(IScheduleData databaseVersion, bool discardMyChanges);
		IPersistableScheduleData SolveConflictBecauseOfExternalDeletion(Guid id, bool discardMyChanges);
	}
}