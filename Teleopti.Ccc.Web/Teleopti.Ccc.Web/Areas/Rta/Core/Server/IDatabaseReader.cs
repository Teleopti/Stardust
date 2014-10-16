using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public interface IDatabaseReader : IGetCurrentActualAgentState
	{
		ConcurrentDictionary<string, int> Datasources();
		ConcurrentDictionary<string, IEnumerable<PersonWithBusinessUnit>> ExternalLogOns();
		ConcurrentDictionary<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>> StateGroups();
		ConcurrentDictionary<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>> ActivityAlarms();

		IList<ScheduleLayer> GetCurrentSchedule(Guid personId);
		IEnumerable<IActualAgentState> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId);
	}

	public interface IGetCurrentActualAgentState
	{
		IActualAgentState GetCurrentActualAgentState(Guid personId);
	}
}