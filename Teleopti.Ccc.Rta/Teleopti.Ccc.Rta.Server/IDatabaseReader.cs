using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server
{
    public interface IDatabaseReader
    {
        IActualAgentState LoadOldState(Guid personToLoad);
        ConcurrentDictionary<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>> StateGroups();
        ConcurrentDictionary<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>> ActivityAlarms();
        IList<ScheduleLayer> GetReadModel(Guid personId);
        IList<IActualAgentState> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId);
    }
}