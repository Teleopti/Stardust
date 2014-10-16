﻿using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class RtaDataHandler : IRtaDataHandler
	{
		private static readonly ILog LoggingSvc = LogManager.GetLogger(typeof (IRtaDataHandler));
		private readonly IEnumerable<IActualAgentStateHasBeenSent> _actualAgentStateHasBeenSent;

		private readonly IActualAgentAssembler _agentAssembler;
		private readonly IDatabaseWriter _databaseWriter;
		private readonly ISignalRClient _messageClient;
		private readonly IMessageSender _messageSender;
		private readonly IDataSourceResolver _dataSourceResolver;
		private readonly IPersonResolver _personResolver;

		public RtaDataHandler(
			ISignalRClient messageClient,
			IMessageSender messageSender,
			IDataSourceResolver dataSourceResolver,
			IPersonResolver personResolver,
			IActualAgentAssembler agentAssembler,
			IDatabaseWriter databaseWriter,
			IEnumerable<IActualAgentStateHasBeenSent> actualAgentStateHasBeenSent)
		{
			_messageClient = messageClient;
			_messageSender = messageSender;
			_dataSourceResolver = dataSourceResolver;
			_personResolver = personResolver;
			_agentAssembler = agentAssembler;
			_databaseWriter = databaseWriter;
			_actualAgentStateHasBeenSent = actualAgentStateHasBeenSent;
		}

		public void ProcessScheduleUpdate(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			_agentAssembler.InvalidateReadModelCache(personId);
			var agentState = _agentAssembler.GetAgentStateForScheduleUpdate(personId, businessUnitId, timestamp);

			if (agentState == null)
				return;

				_databaseWriter.PersistActualAgentState(agentState);
			sendRtaState(agentState);
		}

		public bool IsAlive
		{
			get { return _messageClient.IsAlive; }
		}

		// Probably a WaitHandle object isnt a best choice, but same applies to QueueUserWorkItem method.
		// An alternative using Tasks should be looked at instead.
		public int ProcessRtaData(string logOn, string stateCode, TimeSpan timeInState, DateTime timestamp, Guid platformTypeId, string sourceId, DateTime batchId, bool isSnapshot)
		{
			int dataSourceId;
			var batch = isSnapshot
				? batchId
				: (DateTime?) null;

			if (!_dataSourceResolver.TryResolveId(sourceId, out dataSourceId))
			{
				LoggingSvc.WarnFormat(
					"No data source available for source id = {0}. Event will not be handled before data source is set up.", sourceId);
				return 0;
			}

			if (isSnapshot && string.IsNullOrEmpty(logOn))
			{
				LoggingSvc.InfoFormat("Last of batch detected, initializing handling for batch id: {0}, source id: {1}", batchId,
					sourceId);
				handleLastOfBatch(batchId, sourceId);
				return 0;
			}

			IEnumerable<PersonWithBusinessUnit> personWithBusinessUnits;
			if (!_personResolver.TryResolveId(dataSourceId, logOn, out personWithBusinessUnits))
			{
				LoggingSvc.InfoFormat(
					"No person available for datasource id = {0} and log on {1}. Event will not be handled before person is set up.",
					dataSourceId, logOn);
				return 0;
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
					_databaseWriter.PersistActualAgentState(agentState);
				if (agentState.SendOverMessageBroker)
					sendRtaState(agentState);
			}
			return 1;
		}

		private void handleLastOfBatch(DateTime batchId, string sourceId)
		{
			var missingAgents = _agentAssembler.GetAgentStatesForMissingAgents(batchId, sourceId);
			foreach (var agent in missingAgents.Where(agent => agent != null))
			{
				_databaseWriter.PersistActualAgentState(agent);
				sendRtaState(agent);
			}
		}

		private void sendRtaState(IActualAgentState agentState)
		{
			LoggingSvc.InfoFormat("Adding message to message broker queue AgentState: {0} ", agentState);

			var notification = NotificationFactory.CreateNotification(agentState);

			_messageSender.Send(notification);
			if (_actualAgentStateHasBeenSent != null)
			{
				foreach (var actualAgentStateHasBeenSent in _actualAgentStateHasBeenSent)
				{
					actualAgentStateHasBeenSent.Invoke(agentState);
				}
			}
		}
	}
}
