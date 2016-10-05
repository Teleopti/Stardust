using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public enum DeadLockVictim
	{
		Yes,
		No
	}

	public interface IAgentStatePersister
	{
		// maintainer stuff
		void Delete(Guid personId, DeadLockVictim deadLockVictim);
		void Prepare(AgentStatePrepare model, DeadLockVictim deadLockVictim);
		void InvalidateSchedules(Guid personId, DeadLockVictim deadLockVictim);

		// rta service stuff
		IEnumerable<ExternalLogon> FindAll();
		IEnumerable<ExternalLogon> FindForClosingSnapshot(DateTime snapshotId, string sourceId, string loggedOutState);
		IEnumerable<AgentStateFound> Find(ExternalLogon externalLogon, DeadLockVictim deadLockVictim);
		IEnumerable<AgentStateFound> Find(IEnumerable<ExternalLogon> externalLogons, DeadLockVictim deadLockVictim);
		IEnumerable<AgentState> GetStates();
		void Update(AgentState model);

		[RemoveMeWithToggle(Toggles.RTA_ScheduleQueryOptimization_40260)]
		AgentState Get(Guid personId);
		[RemoveMeWithToggle(Toggles.RTA_PersonOrganizationQueryOptimization_40261)]
		IEnumerable<AgentState> GetStatesNotInSnapshot(DateTime snapshotId, string sourceId);
		[RemoveMeWithToggle(Toggles.RTA_PersonOrganizationQueryOptimization_40261)]
		IEnumerable<AgentState> Get(IEnumerable<Guid> personIds);

	}
}