using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStatePersister
	{
		// maintainer stuff
		void Delete(Guid personId);
		void Prepare(AgentStatePrepare model);
		void InvalidateSchedules(Guid personId);

		// query states by external logon
		IEnumerable<AgentStateFound> Find(int dataSourceId, string userCode);
		IEnumerable<AgentStateFound> Find(int dataSourceId, IEnumerable<string> userCodes);
		IEnumerable<Guid> GetAllPersonIds();
		AgentState Get(Guid personId);
		IEnumerable<AgentState> GetStates();
		IEnumerable<AgentState> GetStatesNotInSnapshot(DateTime snapshotId, string sourceId);
		void Update(AgentState model);

		[RemoveMeWithToggle(Toggles.RTA_PersonOrganizationQueryOptimization_40261)]
		IEnumerable<AgentState> Get(IEnumerable<Guid> personIds);
	}
}