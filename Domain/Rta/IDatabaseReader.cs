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
		ConcurrentDictionary<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>> StateGroups();
		ConcurrentDictionary<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>> ActivityAlarms();

		IList<ScheduleLayer> GetCurrentSchedule(Guid personId);
		IEnumerable<IActualAgentState> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId);
	}

	public interface IReadActualAgentStates
	{
		IActualAgentState GetCurrentActualAgentState(Guid personId);
		IEnumerable<IActualAgentState> GetActualAgentStates();
	}
}