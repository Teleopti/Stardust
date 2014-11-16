using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using log4net;
using MbCache.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Web.Areas.Rta
{
	public interface IRta
	{
		int SaveState(ExternalUserStateInputModel state);
		int SaveStateBatch(IEnumerable<ExternalUserStateInputModel> states);
		int SaveStateSnapshot(IEnumerable<ExternalUserStateInputModel> states);
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

		public Rta(ISignalRClient signalRClient, IMessageSender messageSender, IDatabaseReader databaseReader, IDatabaseWriter databaseWriter, IMbCacheFactory cacheFactory, IAdherenceAggregator adherenceAggregator, IRtaEventPublisher rtaEventPublisher, INow now, IConfigReader configReader)
		{
			_rtaDataHandler = new RtaDataHandler(
				signalRClient,
				messageSender,
				databaseReader,
				databaseWriter,
				cacheFactory,
				adherenceAggregator,
				rtaEventPublisher);
			_now = now;

			Log.Info("The real time adherence service is now started");
			_authenticationKey = configReader.AppSettings["AuthenticationKey"];
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
				BatchId = state.BatchId,
				Timestamp = state.Timestamp
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

		public int SaveState(ExternalUserStateInputModel state)
		{
			var messageId = Guid.NewGuid();

			verifyAuthenticationKey(state.AuthenticationKey, messageId);

			Log.InfoFormat(System.Globalization.CultureInfo.InvariantCulture,
						   "Incoming message: MessageId = {10}, UserCode = {0}, StateCode = {1}, StateDescription = {2}, IsLoggedOn = {3}, SecondsInState = {4}, TimeStamp = {5}, PlatformTypeId = {6}, SourceId = {7}, BatchId = {8}, IsSnapshot = {9}.",
						   state.UserCode, state.StateCode, state.StateDescription, state.IsLoggedOn, state.SecondsInState, state.Timestamp,
						   state.PlatformTypeId, state.SourceId, state.BatchId, state.IsSnapshot, messageId);

			if (string.IsNullOrEmpty(state.SourceId))
			{
				Log.ErrorFormat("The source id was not valid. Supplied value was {0}. (MessageId = {1})", state.SourceId, messageId);
				return -300;
			}

			if (string.IsNullOrEmpty(state.PlatformTypeId))
			{
				Log.ErrorFormat("The platform type id cannot be empty or null. (MessageId = {0})", messageId);
				return -200;
			}

			var parsedPlatformTypeId = new Guid(state.PlatformTypeId);

			if (!state.IsLoggedOn)
			{
				//If the user isn't logged on we'll substitute the stateCode to reflect this
				Log.InfoFormat(
					"This is a log out state. The original state code {0} is substituted with hardcoded state code {1}. (MessageId = {2})",
					state.StateCode, LogOutStateCode, messageId);
				state.StateCode = LogOutStateCode;
			}

			//The DateTimeKind.Utc is not set automatically when deserialising from soap message
			state.Timestamp = DateTime.SpecifyKind(state.Timestamp, DateTimeKind.Utc);
			if (Math.Abs(state.Timestamp.Subtract(_now.UtcDateTime()).TotalMinutes) > 30)
			{
				Log.ErrorFormat(
					"The supplied time stamp should be sent as UTC. Current UTC time is {0} and the supplied timestamp was {1}. (MessageId = {2})",
					_now.UtcDateTime(), state.Timestamp, messageId);
				return -400;
			}

			const int stateCodeMaxLength = 25;
			state.StateCode = state.StateCode.Trim();
			if (state.StateCode.Length > stateCodeMaxLength)
			{
				var newStateCode = state.StateCode.Substring(0, stateCodeMaxLength);
				Log.WarnFormat("The original state code {0} is too long and substituted with state code {1}. (MessageId = {2})",
							   state.StateCode, newStateCode, messageId);
				state.StateCode = newStateCode;
			}

			Log.InfoFormat(
				"Message verified and validated from sender for UserCode: {0}, StateCode: {1}. (MessageId = {2})", state.UserCode, state.StateCode, messageId);
			var result = _rtaDataHandler.ProcessRtaData(state.UserCode.Trim(), state.StateCode, TimeSpan.FromSeconds(state.SecondsInState), state.Timestamp,
										   parsedPlatformTypeId, state.SourceId, state.BatchId, state.IsSnapshot);

			Log.InfoFormat("Message handling complete for UserCode: {0}, StateCode: {1}. (MessageId = {2})", state.UserCode, state.StateCode, messageId);

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
			throw new FaultException("You supplied an invalid authentication key. Please verify the key and try again.");
		}

		public void CheckForActivityChange(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			CheckForActivityChange(new CheckForActivityChangeInputModel
			{
				PersonId = personId,
				BusinessUnitId = businessUnitId,
				Timestamp = timestamp
			});
		}

		public void CheckForActivityChange(CheckForActivityChangeInputModel input)
		{
			Log.InfoFormat("Recieved message from servicebus to check schedule for Person: {0}, BusinessUnit: {1}, Timestamp: {2}", input.PersonId, input.BusinessUnitId, input.Timestamp);

			_rtaDataHandler.ProcessScheduleUpdate(input.PersonId, input.BusinessUnitId, input.Timestamp);
		}

		private static void verifyBatchNotTooLarge(IEnumerable<ExternalUserStateInputModel> externalUserStateBatch)
		{
			if (externalUserStateBatch.Count() <= 50) return;
			Log.ErrorFormat("The incoming batch contains more than 50 external user states. Reduce the number if states per batch to a number below 50.");
			throw new FaultException(
				"Incoming batch too large. Please lower the number of user states in a batch to below 50.");
		}

	}
}
