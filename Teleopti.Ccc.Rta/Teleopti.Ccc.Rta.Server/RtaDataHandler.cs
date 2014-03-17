using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Rta.Server.Resolvers;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.SignalR;
using log4net;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Ccc.Rta.Server
{
	public class RtaDataHandler : IRtaDataHandler
	{
		private static readonly ILog LoggingSvc = LogManager.GetLogger(typeof(IRtaDataHandler));
		private static IActualAgentStateCache _stateCache;
		private readonly IEnumerable<IActualAgentStateHasBeenSent> _afterSends;

		private readonly IActualAgentAssembler _agentAssembler;
		private readonly IMessageSender _asyncMessageSender;
		private readonly IDataSourceResolver _dataSourceResolver;
		private readonly IPersonResolver _personResolver;

		public RtaDataHandler(IMessageSender asyncMessageSender,
		                      IDataSourceResolver dataSourceResolver,
		                      IPersonResolver personResolver,
		                      IActualAgentAssembler agentAssembler,
		                      IActualAgentStateCache stateCache,
		                      IEnumerable<IActualAgentStateHasBeenSent> afterSends)
		{
			_asyncMessageSender = asyncMessageSender;
			_dataSourceResolver = dataSourceResolver;
			_personResolver = personResolver;
			_agentAssembler = agentAssembler;
			_stateCache = stateCache;
			_afterSends = afterSends;

			if (_asyncMessageSender == null) return;

			try
			{
				_asyncMessageSender.StartBrokerService();
			}
			catch (BrokerNotInstantiatedException ex)
			{
				LoggingSvc.Error(
					"The message broker will be unavailable until this service is restarted and initialized with correct parameters",
					ex);
			}
		}

		public void ProcessScheduleUpdate(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			_agentAssembler.InvalidateReadModelCache(personId);
			var agentState = _agentAssembler.GetAgentStateForScheduleUpdate(personId, businessUnitId, timestamp);

			if (agentState == null)
				return;

			_stateCache.AddAgentStateToCache(agentState);
			sendRtaState(agentState);
		}

		public bool IsAlive
		{
			get { return _asyncMessageSender.IsAlive; }
		}

		// Probably a WaitHandle object isnt a best choice, but same applies to QueueUserWorkItem method.
		// An alternative using Tasks should be looked at instead.
		public void ProcessRtaData(string logOn, string stateCode, TimeSpan timeInState, DateTime timestamp,
		                           Guid platformTypeId, string sourceId, DateTime batchId, bool isSnapshot)
		{
			int dataSourceId;
			var batch = isSnapshot
				            ? batchId
				            : (DateTime?) null;

			if (!_dataSourceResolver.TryResolveId(sourceId, out dataSourceId))
			{
				LoggingSvc.WarnFormat(
					"No data source available for source id = {0}. Event will not be handled before data source is set up.", sourceId);
				return;
			}

			if (isSnapshot && string.IsNullOrEmpty(logOn))
			{
				LoggingSvc.InfoFormat("Last of batch detected, initializing handling for batch id: {0}, source id: {1}", batchId,
				                      sourceId);
				handleLastOfBatch(batchId, sourceId);
				return;
			}

			IEnumerable<PersonWithBusinessUnit> personWithBusinessUnits;
			if (!_personResolver.TryResolveId(dataSourceId, logOn, out personWithBusinessUnits))
			{
				LoggingSvc.InfoFormat(
					"No person available for datasource id = {0} and log on {1}. Event will not be handled before person is set up.",
					dataSourceId, logOn);
				return;
			}

			foreach (var personWithBusinessUnit in personWithBusinessUnits)
			{
				LoggingSvc.DebugFormat("ACD-Logon: {0} is connected to PersonId: {1}", logOn, personWithBusinessUnit.PersonId);

				var agentState = _agentAssembler.GetAgentState(personWithBusinessUnit.PersonId,
				                                               personWithBusinessUnit.BusinessUnitId,
				                                               platformTypeId,
				                                               stateCode,
				                                               timestamp,
				                                               timeInState,
				                                               batch,
				                                               sourceId);
				if (agentState == null)
				{
					LoggingSvc.WarnFormat(
						"Could not get state for Person: {0}, Business unit: {1}, PlatformTypeId: {2}, StateCode: {3}, Timestamp: {4}, Time in state: {5}, Batch {6}, SourceId{7}",
						personWithBusinessUnit.PersonId, personWithBusinessUnit.BusinessUnitId, platformTypeId, stateCode, timestamp,
						timeInState, batchId, sourceId);
					continue;
				}
				LoggingSvc.InfoFormat("AgentState built for UserCode: {0}, StateCode: {1}, AgentState: {2}", logOn, stateCode,
				                      agentState);
				_stateCache.AddAgentStateToCache(agentState);
				if (agentState.SendOverMessageBroker)
					sendRtaState(agentState);
			}
		}

		private void handleLastOfBatch(DateTime batchId, string sourceId)
		{
			_stateCache.FlushCacheToDatabase();
			var missingAgents = _agentAssembler.GetAgentStatesForMissingAgents(batchId, sourceId);
			foreach (var agent in missingAgents.Where(agent => agent != null))
			{
				_stateCache.AddAgentStateToCache(agent);
				sendRtaState(agent);
			}
		}

		private void sendRtaState(IActualAgentState agentState)
		{
			LoggingSvc.InfoFormat("Adding message to message broker queue AgentState: {0} ", agentState);

			var notification = NotificationFactory.CreateNotification(agentState);

			_asyncMessageSender.SendNotification(notification);
			if (_afterSends != null)
			{
				_afterSends.ToList().ForEach(s => s.Invoke(agentState));
			}
		}
	}
}
