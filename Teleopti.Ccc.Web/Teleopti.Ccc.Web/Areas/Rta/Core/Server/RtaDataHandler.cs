using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using MbCache.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class RtaDataHandler
	{
		private static readonly ILog loggingSvc = LogManager.GetLogger(typeof(RtaDataHandler));
		private readonly IAdherenceAggregator _adherenceAggregator;

		private readonly IMbCacheFactory _mbCacheFactory;
		private readonly IAlarmFinder _alarmFinder;
		private readonly RtaProcessor _processor;
		private readonly IDatabaseReader _databaseReader;
		private readonly PersonResolver _personResolver;
		
		public RtaDataHandler(
			IAdherenceAggregator adherenceAggregator,
			IDatabaseReader databaseReader,
			IAlarmFinder alarmFinder,
			IMbCacheFactory mbCacheFactory,
			RtaProcessor processor
			)
		{
			_databaseReader = databaseReader;
			_alarmFinder = alarmFinder;
			_personResolver = new PersonResolver(databaseReader);
			_mbCacheFactory = mbCacheFactory;
			_processor = processor;
			_adherenceAggregator = adherenceAggregator;
		}

		public void CheckForActivityChange(Guid personId, Guid businessUnitId)
		{
			_mbCacheFactory.Invalidate(_databaseReader, x => x.GetCurrentSchedule(personId), true);
			process(
				new ExternalUserStateInputModel(),
				personId,
				businessUnitId
				);
		}

		public int ProcessStateChange(ExternalUserStateInputModel input, int dataSourceId)
		{
			IEnumerable<ResolvedPerson> personWithBusinessUnits;
			if (!_personResolver.TryResolveId(dataSourceId, input.UserCode, out personWithBusinessUnits))
			{
				loggingSvc.InfoFormat("No person available for datasource id = {0} and log on {1}. Event will not be handled before person is set up.", dataSourceId, input.UserCode);
				return 0;
			}

			//GLHF
			foreach (var p in personWithBusinessUnits)
			{
				loggingSvc.DebugFormat("ACD-Logon: {0} is connected to PersonId: {1}", input.UserCode, p.PersonId);
				process(input, p.PersonId, p.BusinessUnitId);
			}
			return 1;
		}

		public int CloseSnapshot(ExternalUserStateInputModel input, int dataSourceId)
		{
			var sourceId = input.SourceId;
			var batchId = input.BatchId;

			loggingSvc.InfoFormat("Last of batch detected, initializing handling for batch id: {0}, source id: {1}", batchId, sourceId);
			var missingAgents = getAgentStatesForMissingAgents(batchId, sourceId);
			foreach (var agent in missingAgents)
			{
				input.StateCode = "CCC Logged out";
				input.PlatformTypeId = Guid.Empty.ToString();
				process(input, agent.PersonId, agent.BusinessUnitId);
			}
			return 1;
		}

		private IEnumerable<AgentStateReadModel> getAgentStatesForMissingAgents(DateTime batchid, string sourceId)
		{
			var missingAgents = _databaseReader.GetMissingAgentStatesFromBatch(batchid, sourceId);
			var agentsNotAlreadyLoggedOut = from a in missingAgents
											where !isAgentLoggedOut(a.StateCode,
												a.PlatformTypeId,
												a.BusinessUnitId)
											select a;
			return agentsNotAlreadyLoggedOut;
		}

		private bool isAgentLoggedOut(string stateCode, Guid platformTypeId, Guid businessUnitId)
		{
			var state = _alarmFinder.GetStateGroup(stateCode, platformTypeId, businessUnitId);
			return state != null && state.IsLogOutState;
		}

		private void process(
			ExternalUserStateInputModel input,
			Guid personId,
			Guid businessUnitId
			)
		{
			_processor.Process(input, personId, businessUnitId);
		}

		public void Initialize()
		{
			_adherenceAggregator.Initialize();
		}
	}

}
