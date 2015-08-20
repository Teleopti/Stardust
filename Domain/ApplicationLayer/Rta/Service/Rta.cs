﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Resolvers;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IRta
	{
		int SaveState(ExternalUserStateInputModel input);
		int SaveStateBatch(IEnumerable<ExternalUserStateInputModel> states);
		int SaveStateSnapshot(IEnumerable<ExternalUserStateInputModel> states);
		void CheckForActivityChange(CheckForActivityChangeInputModel input);
		void Initialize();
	}

	public class Rta : IRta
	{
		private readonly IAgentStateReadModelReader _agentStateReadModelReader;
		private readonly string _authenticationKey;
		public static string LogOutStateCode = "LOGGED-OFF";
		private static readonly ILog Log = LogManager.GetLogger(typeof(Rta));

		public static string DefaultAuthenticationKey = "!#¤atAbgT%";
		private readonly DataSourceResolver _dataSourceResolver;
		private readonly ICacheInvalidator _cacheInvalidator;
		private readonly RtaProcessor _processor;
		private readonly INow _now;
		private readonly IPersonOrganizationProvider _personOrganizationProvider;
		private readonly IAgentStateReadModelUpdater _agentStateReadModelUpdater;
		private readonly IAgentStateMessageSender _messageSender;
		private readonly AgentStateAssembler _agentStateAssembler;
		private readonly IAdherenceAggregator _adherenceAggregator;
		private readonly PersonResolver _personResolver;
		private readonly IStateMapper _stateMapper;

		public Rta(
			IAdherenceAggregator adherenceAggregator,
			IDatabaseReader databaseReader, 
			IAgentStateReadModelReader agentStateReadModelReader, 
			ICacheInvalidator cacheInvalidator,
			IStateMapper stateMapper,
			INow now, 
			IConfigReader configReader,
			IAgentStateReadModelUpdater agentStateReadModelUpdater,
			IAgentStateMessageSender messageSender,
			IPersonOrganizationProvider personOrganizationProvider,
			RtaProcessor processor,
			AgentStateAssembler agentStateAssembler
			)
		{
			_agentStateReadModelReader = agentStateReadModelReader;
			_dataSourceResolver = new DataSourceResolver(databaseReader);
			_stateMapper = stateMapper;
			_cacheInvalidator = cacheInvalidator;
			_processor = processor;
			_now = now;
			_personOrganizationProvider = personOrganizationProvider;
			_agentStateReadModelUpdater = agentStateReadModelUpdater;
			_messageSender = messageSender;
			_agentStateAssembler = agentStateAssembler;
			_adherenceAggregator = adherenceAggregator;
			_personResolver = new PersonResolver(databaseReader);

			Log.Info("The real time adherence service is now started");
			_authenticationKey = configReader.AppConfig("AuthenticationKey");
			if (string.IsNullOrEmpty(_authenticationKey))
				_authenticationKey = DefaultAuthenticationKey;
		}

		public int SaveStateSnapshot(IEnumerable<ExternalUserStateInputModel> states)
		{
			var state = states.First();
			SaveStateBatch(states);
			return SaveState(new ExternalUserStateInputModel
			{
				AuthenticationKey = state.AuthenticationKey,
				PlatformTypeId = state.PlatformTypeId,
				SourceId = state.SourceId,
				UserCode = "",
				IsSnapshot = true,
				BatchId = state.BatchId
			});
		}

		public int SaveStateBatch(IEnumerable<ExternalUserStateInputModel> states)
		{
			verifyBatchNotTooLarge(states);

			var result = 0;

			foreach (var state in states)
			{
				var processResult = SaveState(state);

				if (processResult < result || result == 0)
					result = processResult;
			}

			return result;
		}

		public int SaveState(ExternalUserStateInputModel input)
		{
			var messageId = Guid.NewGuid();

			verifyAuthenticationKey(input.AuthenticationKey, messageId);

			Log.InfoFormat(CultureInfo.InvariantCulture,
						   "Incoming message: MessageId: {8}, UserCode: {0}, StateCode: {1}, StateDescription: {2}, IsLoggedOn: {3}, PlatformTypeId: {4}, SourceId: {5}, BatchId: {6}, IsSnapshot: {7}.",
						   input.UserCode, input.StateCode, input.StateDescription, input.IsLoggedOn, input.PlatformTypeId, input.SourceId, input.BatchId, input.IsSnapshot, messageId);

			if (string.IsNullOrEmpty(input.SourceId))
			{
				Log.ErrorFormat("The source id was not valid. Supplied value was {0}. (MessageId: {1})", input.SourceId, messageId);
				return -300;
			}

			if (string.IsNullOrEmpty(input.PlatformTypeId))
			{
				Log.ErrorFormat("The platform type id cannot be empty or null. (MessageId: {0})", messageId);
				return -200;
			}

			if (!input.IsLoggedOn)
			{
				//If the user isn't logged on we'll substitute the stateCode to reflect this
				Log.InfoFormat("This is a log out state. The original state code {0} is substituted with hardcoded state code {1}. (MessageId: {2})", input.StateCode, LogOutStateCode, messageId);
				input.StateCode = LogOutStateCode;
			}

			const int stateCodeMaxLength = 25;
			input.StateCode = input.StateCode.Trim();
			if (input.StateCode.Length > stateCodeMaxLength)
			{
				var newStateCode = input.StateCode.Substring(0, stateCodeMaxLength);
				Log.WarnFormat("The original state code {0} is too long and substituted with state code {1}. (MessageId: {2})", input.StateCode, newStateCode, messageId);
				input.StateCode = newStateCode;
			}

			Log.InfoFormat("Message verified and validated from sender for UserCode: {0}, StateCode: {1}. (MessageId: {2})", input.UserCode, input.StateCode, messageId);

			int dataSourceId;
			if (!_dataSourceResolver.TryResolveId(input.SourceId, out dataSourceId))
			{
				Log.WarnFormat("No data source available for source id: {0}. Event will not be handled before data source is set up.", input.SourceId);
				return 0;
			}

			input.UserCode = input.UserCode.Trim();

			int result;
			if (input.IsSnapshot && string.IsNullOrEmpty(input.UserCode))
			{
				result = closeSnapshot(input, dataSourceId);
			}
			else
			{
				result = processStateChange(input, dataSourceId);
			}

			Log.InfoFormat("Message handling complete for UserCode: {0}, StateCode: {1}. (MessageId: {2})", input.UserCode, input.StateCode, messageId);

			return result;
		}

		private int closeSnapshot(ExternalUserStateInputModel input, int dataSourceId)
		{
			input.StateCode = "CCC Logged out";
			input.PlatformTypeId = Guid.Empty.ToString();
			var missingAgents = _agentStateReadModelReader.GetMissingAgentStatesFromBatch(input.BatchId, input.SourceId);
			var agentsNotAlreadyLoggedOut = from a in missingAgents
											let state = _stateMapper.StateFor(a.BusinessUnitId, a.PlatformTypeId, a.StateCode, null)
											where !state.IsLogOutState
											select a;
			foreach (var agent in agentsNotAlreadyLoggedOut)
				process(input, agent.PersonId, agent.BusinessUnitId);
			return 1;
		}

		private int processStateChange(ExternalUserStateInputModel input, int dataSourceId)
		{
			IEnumerable<ResolvedPerson> personWithBusinessUnits;
			if (!_personResolver.TryResolveId(dataSourceId, input.UserCode, out personWithBusinessUnits))
			{
				Log.InfoFormat("No person available for datasource id: {0} and UserCode: {1}", dataSourceId, input.UserCode);
				return 0;
			}

			//GLHF
			foreach (var p in personWithBusinessUnits)
			{
				Log.DebugFormat("UserCode: {0} is connected to PersonId: {1}", input.UserCode, p.PersonId);
				process(input, p.PersonId, p.BusinessUnitId);
			}
			return 1;
		}

		private void verifyAuthenticationKey(string authenticationKey, Guid messageId)
		{
			if (authenticationKey == _authenticationKey)
				return;

			// for test ShouldAcceptIfThirdAndFourthLetterOfAuthenticationKeyIsCorrupted_BecauseOfEncodingIssuesWithThe3rdLetterOfTheDefaultKey
			if (authenticationKey.Remove(2, 1) == _authenticationKey.Remove(2, 2))
				return;

			Log.ErrorFormat("An invalid authentication key was supplied. AuthenticationKey: {0}. (MessageId: {1})", authenticationKey, messageId);
			throw new InvalidAuthenticationKeyException("You supplied an invalid authentication key. Please verify the key and try again.");
		}

		[InfoLog]
		public void CheckForActivityChange(CheckForActivityChangeInputModel input)
		{
			_cacheInvalidator.InvalidateSchedules(input.PersonId);
			process(
				null,
				input.PersonId,
				input.BusinessUnitId
				);
		}

		private static void verifyBatchNotTooLarge(IEnumerable<ExternalUserStateInputModel> externalUserStateBatch)
		{
			if (externalUserStateBatch.Count() <= 50) return;
			Log.ErrorFormat("The incoming batch contains more than 50 external user states. Reduce the number if states per batch to a number below 50.");
			throw new BatchTooBigException("Incoming batch too large. Please lower the number of user states in a batch to below 50.");
		}

		private void process(
			ExternalUserStateInputModel input,
			Guid personId,
			Guid businessUnitId
			)
		{
			var currentTime = _now.UtcDateTime();
			_processor.Process(
				new RtaProcessContext(
					input,
					personId,
					businessUnitId,
					currentTime,
					_personOrganizationProvider,
					_agentStateReadModelUpdater,
					_messageSender,
					_adherenceAggregator,
					() => _agentStateAssembler.MakePreviousState(personId, _agentStateReadModelReader.GetCurrentActualAgentState(personId)),
					(scheduleInfo, context) => _agentStateAssembler.MakeCurrentState(scheduleInfo, context.Input, context.Person, context.PreviousState(scheduleInfo), currentTime)
					));
		}

		public void Initialize()
		{
			_adherenceAggregator.Initialize();
		}
	}

	public class BatchTooBigException : Exception
	{
		public BatchTooBigException(string message) : base(message)
		{
		}
	}

	public class InvalidAuthenticationKeyException : Exception
	{
		public InvalidAuthenticationKeyException(string message) : base(message)
		{
		}
	}
}
