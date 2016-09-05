using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStatePersister
	{
		// cleaner/updater/preparer stuff
		void Delete(Guid personId);
		void Prepare(AgentStatePrepare model);

		// query states by external logon
		IEnumerable<AgentStateFound> Find(int dataSourceId, string userCode);
		IEnumerable<AgentStateFound> Find(int dataSourceId, IEnumerable<string> userCodes);
		// query states for each person
		IEnumerable<Guid> GetPersonIds();
		IEnumerable<AgentState> GetStates();
		IEnumerable<AgentState> GetStatesNotInSnapshot(DateTime snapshotId, string sourceId);
		void Update(AgentState model);

		// legacy queries, by person id
		[RemoveMeWithToggle(Toggles.RTA_PersonOrganizationQueryOptimization_40261)]
		AgentState Get(Guid personId);
		[RemoveMeWithToggle(Toggles.RTA_PersonOrganizationQueryOptimization_40261)]
		IEnumerable<AgentState> Get(IEnumerable<Guid> personIds);

	}
}