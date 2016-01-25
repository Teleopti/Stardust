using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Aspects;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Resolvers;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class Rta
	{
		private readonly IAgentStateReadModelReader _agentStateReadModelReader;
		private readonly IPreviousStateInfoLoader _previousStateInfoLoader;
		public static string LogOutStateCode = "LOGGED-OFF";

		private readonly DataSourceResolver _dataSourceResolver;
		private readonly ICacheInvalidator _cacheInvalidator;
		private readonly RtaProcessor _processor;
		private readonly IAuthenticator _authenticator;
		private readonly RtaInitializor _initializor;
		private readonly ActivityChangeProcessor _activityChangeProcessor;
		private readonly IPersonLoader _personLoader;
		private readonly INow _now;
		private readonly IAgentStateReadModelUpdater _agentStateReadModelUpdater;
		private readonly IAgentStateMessageSender _messageSender;
		private readonly IAdherenceAggregator _adherenceAggregator;
		private readonly IStateMapper _stateMapper;

		public Rta(
			IAdherenceAggregator adherenceAggregator,
			IAgentStateReadModelReader agentStateReadModelReader,
			IPreviousStateInfoLoader previousStateInfoLoader,
			ICacheInvalidator cacheInvalidator,
			IStateMapper stateMapper,
			INow now,
			IAgentStateReadModelUpdater agentStateReadModelUpdater,
			IAgentStateMessageSender messageSender,
			IDatabaseLoader databaseLoader,
			RtaProcessor processor,
			IAuthenticator authenticator,
			RtaInitializor initializor,
			ActivityChangeProcessor activityChangeProcessor,
			IPersonLoader personLoader)
		{
			_agentStateReadModelReader = agentStateReadModelReader;
			_previousStateInfoLoader = previousStateInfoLoader;
			_dataSourceResolver = new DataSourceResolver(databaseLoader);
			_stateMapper = stateMapper;
			_cacheInvalidator = cacheInvalidator;
			_processor = processor;
			_authenticator = authenticator;
			_initializor = initializor;
			_activityChangeProcessor = activityChangeProcessor;
			_personLoader = personLoader;
			_now = now;
			_agentStateReadModelUpdater = agentStateReadModelUpdater;
			_messageSender = messageSender;
			_adherenceAggregator = adherenceAggregator;
		}

		[InfoLog]
		[RtaDataSourceScope]
		public virtual void SaveStateSnapshot(IEnumerable<ExternalUserStateInputModel> states)
		{
			_initializor.EnsureTenantInitialized();

			var state = states.First();
			SaveStateBatch(
				states
					.Concat(new[]
					{
						new ExternalUserStateInputModel
						{
							AuthenticationKey = state.AuthenticationKey,
							PlatformTypeId = state.PlatformTypeId,
							SourceId = state.SourceId,
							UserCode = "",
							IsSnapshot = true,
							BatchId = state.BatchId
						}
					})
				);
		}

		[InfoLog]
		[RtaDataSourceScope]
		public virtual void SaveStateBatch(IEnumerable<ExternalUserStateInputModel> states)
		{
			_initializor.EnsureTenantInitialized();

			if (states.Count() > 50)
				throw new BatchTooBigException("Incoming batch too large. Please lower the number of user states in a batch to below 50.");

			var exceptions = new List<Exception>();
			states.ForEach(s =>
			{
				try
				{
					SaveState(s);
				}
				catch (Exception e)
				{
					exceptions.Add(e);
				}
			});
			if (exceptions.Any())
				throw new AggregateException(exceptions);
		}

		[InfoLog]
		[RtaDataSourceScope]
		public virtual void SaveState(ExternalUserStateInputModel input)
		{
			validateAuthenticationKey(input);
			_initializor.EnsureTenantInitialized();
			validatePlatformId(input);
			var dataSourceId = validateSourceId(input);
			ProcessInput(input, dataSourceId);
		}

		private void validateAuthenticationKey(ExternalUserStateInputModel input)
		{
			input.MakeLegacyKeyEncodingSafe();
			if (!_authenticator.Authenticate(input.AuthenticationKey))
				throw new InvalidAuthenticationKeyException(
					"You supplied an invalid authentication key. Please verify the key and try again.");
		}

		private static void validatePlatformId(ExternalUserStateInputModel input)
		{
			if (string.IsNullOrEmpty(input.PlatformTypeId))
				throw new InvalidPlatformException("Platform id is required");
		}

		private int validateSourceId(ExternalUserStateInputModel input)
		{
			if (string.IsNullOrEmpty(input.SourceId))
				throw new InvalidSourceException("Source id is required");
			int dataSourceId;
			if (!_dataSourceResolver.TryResolveId(input.SourceId, out dataSourceId))
				throw new InvalidSourceException(string.Format("Source id not found {0}", input.SourceId));
			return dataSourceId;
		}

		private IEnumerable<PersonOrganizationData> validateUserCode(ExternalUserStateInputModel input, int dataSourceId)
		{
			var persons = _personLoader.LoadPersonData(dataSourceId, input.UserCode);

			if (!persons.Any())
				throw new InvalidUserCodeException(string.Format("No person found for DataSourceId {0} and UserCode {1}", dataSourceId, input.UserCode));
			return persons;
		}

		[InfoLog]
		protected virtual void ProcessInput(ExternalUserStateInputModel input, int dataSourceId)
		{
			input.UserCode = FixUserCode(input);
			input.StateCode = FixStateCode(input);

			if (input.IsSnapshot && string.IsNullOrEmpty(input.UserCode))
				CloseSnapshot(input);
			else
			{
				var persons = validateUserCode(input, dataSourceId);
				//GLHF
				foreach (var person in persons)
					Process(input, person);
			}
		}

		[InfoLog]
		protected virtual string FixUserCode(ExternalUserStateInputModel input)
		{
			return input.UserCode.Trim();
		}

		[InfoLog]
		protected virtual string FixStateCode(ExternalUserStateInputModel input)
		{
			if (!input.IsLoggedOn)
				return LogOutStateCode;
			var stateCode = input.StateCode.Trim();
			const int stateCodeMaxLength = 25;
			if (stateCode.Length > stateCodeMaxLength)
				return stateCode.Substring(0, stateCodeMaxLength);
			return stateCode;
		}
		
		[InfoLog]
		protected virtual void CloseSnapshot(ExternalUserStateInputModel input)
		{
			input.StateCode = "CCC Logged out";
			input.PlatformTypeId = Guid.Empty.ToString();
			var missingAgents = _agentStateReadModelReader.GetMissingAgentStatesFromBatch(input.BatchId, input.SourceId);
			var agentsNotAlreadyLoggedOut = from a in missingAgents
											let state = _stateMapper.StateFor(a.BusinessUnitId, a.PlatformTypeId, a.StateCode, null)
											where !state.IsLogOutState
											select a;

			foreach (var agent in agentsNotAlreadyLoggedOut)
				Process(
					input,
					new PersonOrganizationData
					{
						PersonId = agent.PersonId,
						BusinessUnitId = agent.BusinessUnitId,
						SiteId = agent.SiteId.GetValueOrDefault(),
						TeamId = agent.TeamId.GetValueOrDefault()
					});
		}

		[InfoLog]
		protected virtual void Process(ExternalUserStateInputModel input, PersonOrganizationData person)
		{
			_processor.Process(
				new RtaProcessContext(
					input,
					person,
					_now,
					_agentStateReadModelUpdater,
					_messageSender,
					_adherenceAggregator,
					_previousStateInfoLoader
					));
		}

		[InfoLog]
		[RtaDataSourceScope]
		public virtual void ReloadSchedulesOnNextCheckForActivityChanges(string tenant, Guid personId)
		{
			_cacheInvalidator.InvalidateSchedules(personId);
		}

		[InfoLog]
		[RtaDataSourceScope]
		public virtual void CheckForActivityChanges(string tenant)
		{
			_activityChangeProcessor.CheckForActivityChanges();
		}

		[RtaDataSourceScope]
		public virtual void Touch(string tenant)
		{
			_initializor.EnsureTenantInitialized();
		}

	}
}
