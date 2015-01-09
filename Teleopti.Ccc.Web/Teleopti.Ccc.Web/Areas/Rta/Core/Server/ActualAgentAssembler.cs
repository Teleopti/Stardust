using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class ActualAgentAssembler : IActualAgentAssembler
	{
		private readonly IDatabaseReader _databaseReader;
		private readonly IAlarmFinder _alarmFinder;

		public ActualAgentAssembler(IDatabaseReader databaseReader, IAlarmFinder alarmFinder)
		{
			_databaseReader = databaseReader;
			_alarmFinder = alarmFinder;
		}

		public IEnumerable<IActualAgentState> GetAgentStatesForMissingAgents(DateTime batchid, string sourceId)
		{
			var missingAgents = _databaseReader.GetMissingAgentStatesFromBatch(batchid, sourceId);
			var agentsNotAlreadyLoggedOut = from a in missingAgents
				where !IsAgentLoggedOut(
					a.PersonId,
					a.StateCode,
					a.PlatformTypeId,
					a.BusinessUnitId)
				select a;
			return agentsNotAlreadyLoggedOut;
		}

		public bool IsAgentLoggedOut(Guid personId, string stateCode, Guid platformTypeId, Guid businessUnitId)
		{
			var state = _alarmFinder.GetStateGroup(stateCode, platformTypeId, businessUnitId);
			return state != null && state.IsLogOutState;
		}

	}
}