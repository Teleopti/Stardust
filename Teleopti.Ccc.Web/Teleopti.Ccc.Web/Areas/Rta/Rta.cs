﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using log4net;
using MbCache.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Rta.WebService;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Web.Areas.Rta
{
	public interface IRta
	{
		int SaveState(string authenticationKey, string userCode, string stateCode,
			string stateDescription, bool isLoggedOn, int secondsInState, DateTime timestamp,
			string platformTypeId, string sourceId, DateTime batchId, bool isSnapshot);
		int SaveState(string authenticationKey, string platformTypeId, string sourceId, ExternalUserState state);
		int SaveState(ExternalUserStateInputModel state);

		int SaveStateBatch(string authenticationKey, string platformTypeId, string sourceId, ICollection<ExternalUserState> externalUserStateBatch);
		int SaveStateBatch(IEnumerable<ExternalUserStateInputModel> states);

		int SaveStateSnapshot(IEnumerable<ExternalUserStateInputModel> states);

		void CheckForActivityChange(Guid personId, Guid businessUnitId, DateTime timestamp);
		void CheckForActivityChange(CheckForActivityChangeInputModel input);
	}

	public class Rta : IRta
	{
		private readonly RtaDataHandler _rtaDataHandler;
		private readonly INow _now;
		private readonly string _authenticationKey;
		public static string LogOutStateCode = "LOGGED-OFF";
		private static readonly ILog Log = LogManager.GetLogger(typeof(Rta));

		public static string DefaultAuthenticationKey = "!#¤atAbgT%";

		public Rta(ISignalRClient signalRClient, IMessageSender messageSender, IDatabaseReader databaseReader, IDatabaseWriter databaseWriter, IMbCacheFactory cacheFactory, IAdherenceAggregator adherenceAggregator, IRtaEventPublisher eventPublisher, INow now, IConfigReader configReader)
		{
			_rtaDataHandler = new RtaDataHandler(
				signalRClient,
				messageSender,
				databaseReader,
				databaseWriter,
				cacheFactory,
				adherenceAggregator,
				eventPublisher);
			_now = now;

			Log.Info("The real time adherence service is now started");
			_authenticationKey = configReader.AppSettings["AuthenticationKey"];
			if (string.IsNullOrEmpty(_authenticationKey))
				_authenticationKey = DefaultAuthenticationKey;
		}

		public int SaveState(ExternalUserStateInputModel state)
		{
			return SaveState(state.AuthenticationKey, state.UserCode, state.StateCode, state.StateDescription,
				state.IsLoggedOn, state.SecondsInState, state.Timestamp, state.PlatformTypeId, state.SourceId, state.BatchId, state.IsSnapshot);
		}

		public int SaveStateBatch(IEnumerable<ExternalUserStateInputModel> states)
		{
			var state1 = states.First();
			return SaveStateBatch(state1.AuthenticationKey, state1.PlatformTypeId, state1.SourceId, new Collection<ExternalUserState>(new List<ExternalUserState>(states)));
		}

		public int SaveStateSnapshot(IEnumerable<ExternalUserStateInputModel> states)
		{
			var state = states.First();
			SaveStateBatch(state.AuthenticationKey, state.PlatformTypeId, state.SourceId, new Collection<ExternalUserState>(new List<ExternalUserState>(states)));
			return SaveState(new ExternalUserStateInputModel
			{
				AuthenticationKey = state.AuthenticationKey,
				PlatformTypeId = state.PlatformTypeId,
				SourceId = state.SourceId,
				UserCode = "",
				IsSnapshot = true,
				BatchId = state.BatchId,
				Timestamp = state.Timestamp
			});
		}

		public int SaveState(string authenticationKey, string platformTypeId, string sourceId, ExternalUserState state)
		{
			return SaveState(authenticationKey, state.UserCode, state.StateCode, state.StateDescription,
				state.IsLoggedOn, state.SecondsInState, state.Timestamp, platformTypeId, sourceId, state.BatchId, state.IsSnapshot);
		}

		public int SaveState(string authenticationKey, string userCode, string stateCode,
										 string stateDescription, bool isLoggedOn, int secondsInState, DateTime timestamp,
										 string platformTypeId, string sourceId, DateTime batchId, bool isSnapshot)
		{
			var messageId = Guid.NewGuid();
			verifyAuthenticationKey(authenticationKey, messageId);
			return processExternalUserState(messageId, userCode, stateCode, stateDescription, isLoggedOn, secondsInState, timestamp, platformTypeId, sourceId, batchId, isSnapshot);
		}

		private int processExternalUserState(Guid messageId, string userCode, string stateCode, string stateDescription,
											 bool isLoggedOn, int secondsInState, DateTime timestamp, string platformTypeId,
											 string sourceId, DateTime batchId, bool isSnapshot)
		{
			Log.InfoFormat(System.Globalization.CultureInfo.InvariantCulture,
						   "Incoming message: MessageId = {10}, UserCode = {0}, StateCode = {1}, StateDescription = {2}, IsLoggedOn = {3}, SecondsInState = {4}, TimeStamp = {5}, PlatformTypeId = {6}, SourceId = {7}, BatchId = {8}, IsSnapshot = {9}.",
						   userCode, stateCode, stateDescription, isLoggedOn, secondsInState, timestamp,
						   platformTypeId, sourceId, batchId, isSnapshot, messageId);

			if (string.IsNullOrEmpty(sourceId))
			{
				Log.ErrorFormat("The source id was not valid. Supplied value was {0}. (MessageId = {1})", sourceId, messageId);
				return -300;
			}

			if (string.IsNullOrEmpty(platformTypeId))
			{
				Log.ErrorFormat("The platform type id cannot be empty or null. (MessageId = {0})", messageId);
				return -200;
			}

			var parsedPlatformTypeId = new Guid(platformTypeId);

			if (!isLoggedOn)
			{
				//If the user isn't logged on we'll substitute the stateCode to reflect this
				Log.InfoFormat(
					"This is a log out state. The original state code {0} is substituted with hardcoded state code {1}. (MessageId = {2})",
					stateCode, LogOutStateCode, messageId);
				stateCode = LogOutStateCode;
			}

			//The DateTimeKind.Utc is not set automatically when deserialising from soap message
			timestamp = DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
			if (Math.Abs(timestamp.Subtract(_now.UtcDateTime()).TotalMinutes) > 30)
			{
				Log.ErrorFormat(
					"The supplied time stamp should be sent as UTC. Current UTC time is {0} and the supplied timestamp was {1}. (MessageId = {2})",
					_now.UtcDateTime(), timestamp, messageId);
				return -400;
			}

			const int stateCodeMaxLength = 25;
			stateCode = stateCode.Trim();
			if (stateCode.Length > stateCodeMaxLength)
			{
				var newStateCode = stateCode.Substring(0, stateCodeMaxLength);
				Log.WarnFormat("The original state code {0} is too long and substituted with state code {1}. (MessageId = {2})",
							   stateCode, newStateCode, messageId);
				stateCode = newStateCode;
			}

			Log.InfoFormat(
				"Message verified and validated from sender for UserCode: {0}, StateCode: {1}. (MessageId = {2})", userCode,
				stateCode, messageId);
			var result = _rtaDataHandler.ProcessRtaData(userCode.Trim(), stateCode, TimeSpan.FromSeconds(secondsInState), timestamp,
										   parsedPlatformTypeId, sourceId, batchId, isSnapshot);

			Log.InfoFormat("Message handling complete for UserCode: {0}, StateCode: {1}. (MessageId = {2})", userCode,
						   stateCode, messageId);

			return result;
		}

		private void verifyAuthenticationKey(string authenticationKey, Guid messageId)
		{
			if (authenticationKey == _authenticationKey)
				return;

			// for test ShouldAcceptIfThirdAndFourthLetterOfAuthenticationKeyIsCorrupted_BecauseOfEncodingIssuesWithThe3rdLetterOfTheDefaultKey
			if (authenticationKey.Remove(2, 1) == _authenticationKey.Remove(2, 2))
				return;

			Log.ErrorFormat("An invalid authentication key was supplied. AuthenticationKey = {0}. (MessageId = {1})",
				authenticationKey, messageId);
			throw new FaultException(
				"You supplied an invalid authentication key. Please verify the key and try again.");
		}

		public int SaveStateBatch(string authenticationKey, string platformTypeId, string sourceId, ICollection<ExternalUserState> externalUserStateBatch)
		{
			var messageId = Guid.NewGuid();

			verifyAuthenticationKey(authenticationKey, messageId);

			verifyBatchNotTooLarge(externalUserStateBatch);

			var result = 0;

			foreach (var user in externalUserStateBatch)
			{
				var processResult = SaveState(authenticationKey, user.UserCode, user.StateCode, user.StateDescription,
														  user.IsLoggedOn, user.SecondsInState, user.Timestamp, platformTypeId,
														  sourceId, user.BatchId, user.IsSnapshot);
				if (processResult < result || result == 0)
					result = processResult;
			}

			return result;
		}

		public void CheckForActivityChange(CheckForActivityChangeInputModel input)
		{
			CheckForActivityChange(input.PersonId, input.BusinessUnitId, input.Timestamp);
		}

		public void CheckForActivityChange(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			Log.InfoFormat(
				"Recieved message from servicebus to check schedule for Person: {0}, BusinessUnit: {1}, Timestamp: {2}", personId,
				businessUnitId, timestamp);

			_rtaDataHandler.ProcessScheduleUpdate(personId, businessUnitId, timestamp);
		}

		private static void verifyBatchNotTooLarge(ICollection<ExternalUserState> externalUserStateBatch)
		{
			if (externalUserStateBatch.Count > 50)
			{
				Log.ErrorFormat("The incoming batch contains more than 50 external user states. Reduce the number if states per batch to a number below 50.");
				throw new FaultException(
					"Incoming batch too large. Please lower the number of user states in a batch to below 50.");
			}
		}

	}
}
