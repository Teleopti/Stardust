using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Rta
{
	public class ResolvedPerson
	{
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
	}

	public interface IDatabaseReader : IReadActualAgentStates
	{
		ConcurrentDictionary<string, int> Datasources();
		ConcurrentDictionary<string, IEnumerable<ResolvedPerson>> ExternalLogOns();
		IEnumerable<StateCodeInfo> StateCodeInfos();
		ConcurrentDictionary<Tuple<Guid, Guid, Guid>, List<AlarmMappingInfo>> AlarmMappingInfos();

		IList<ScheduleLayer> GetCurrentSchedule(Guid personId);
		IEnumerable<AgentStateReadModel> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId);
	}

	public interface IReadActualAgentStates
	{
		AgentStateReadModel GetCurrentActualAgentState(Guid personId);
		IEnumerable<AgentStateReadModel> GetActualAgentStates();
	}
}