using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Teleopti.Ccc.Rta.Server.Resolvers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server
{
	public interface ILoadActualAgentState
	{
		IActualAgentState LoadOldState(Guid personToLoad);
	}

	public interface IDatabaseReader : ILoadActualAgentState
	{
		ConcurrentDictionary<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>> StateGroups();
        ConcurrentDictionary<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>> ActivityAlarms();
        IList<ScheduleLayer> GetReadModel(Guid personId);
        IList<IActualAgentState> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId);
	    ConcurrentDictionary<string, IEnumerable<PersonWithBusinessUnit>> LoadAllExternalLogOns();
	    ConcurrentDictionary<string, int> LoadDatasources();
    }
}