using System;
using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.States
{
	public interface IAgentStatePersister
	{
		// maintain
		void Delete(Guid personId, DeadLockVictim deadLockVictim);
		void Prepare(AgentStatePrepare model, DeadLockVictim deadLockVictim);

		// find things to work with
		IEnumerable<PersonForCheck> FindForCheck();
		IEnumerable<Guid> FindForClosingSnapshot(DateTime snapshotId, int snapshotDataSourceId, IEnumerable<Guid> loggedOutStateGroupIds);

		// lock and update
		LockedData LockNLoad(IEnumerable<Guid> personIds, DeadLockVictim deadLockVictim);
		void Update(AgentState model);
	}

	public class LockedData
	{
		public IEnumerable<AgentState> AgentStates;
		public string MappingVersion;
		public CurrentScheduleReadModelVersion ScheduleVersion;
	}

}