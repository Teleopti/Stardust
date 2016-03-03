using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class Rta
	{
		private readonly IAgentStateReadModelReader _agentStateReadModelReader;
		private readonly IPreviousStateInfoLoader _previousStateInfoLoader;
		public static string LogOutStateCode = "LOGGED-OFF";

		private readonly ICacheInvalidator _cacheInvalidator;
		private readonly RtaProcessor _processor;
		private readonly TenantLoader _tenantLoader;
		private readonly RtaInitializor _initializor;
		private readonly ActivityChangeProcessor _activityChangeProcessor;
		private readonly IStateContextLoader _stateContextLoader;
		private readonly INow _now;
		private readonly IAgentStateReadModelUpdater _agentStateReadModelUpdater;
		private readonly IStateMapper _stateMapper;

		public Rta(
			IAgentStateReadModelReader agentStateReadModelReader,
			IPreviousStateInfoLoader previousStateInfoLoader,
			ICacheInvalidator cacheInvalidator,
			IStateMapper stateMapper,
			INow now,
			IAgentStateReadModelUpdater agentStateReadModelUpdater,
			RtaProcessor processor,
			TenantLoader tenantLoader,
			RtaInitializor initializor,
			ActivityChangeProcessor activityChangeProcessor,
			IStateContextLoader stateContextLoader)
		{
			_agentStateReadModelReader = agentStateReadModelReader;
			_previousStateInfoLoader = previousStateInfoLoader;
			_stateMapper = stateMapper;
			_cacheInvalidator = cacheInvalidator;
			_processor = processor;
			_tenantLoader = tenantLoader;
			_initializor = initializor;
			_activityChangeProcessor = activityChangeProcessor;
			_stateContextLoader = stateContextLoader;
			_now = now;
			_agentStateReadModelUpdater = agentStateReadModelUpdater;
		}

		[InfoLog]
		[TenantScope]
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
		[TenantScope]
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
		[TenantScope]
		public virtual void SaveState(ExternalUserStateInputModel input)
		{
			validateAuthenticationKey(input);
			_initializor.EnsureTenantInitialized();
			validatePlatformId(input);
			ProcessInput(input);
		}

		private void validateAuthenticationKey(ExternalUserStateInputModel input)
		{
			input.MakeLegacyKeyEncodingSafe();
			if (!_tenantLoader.Authenticate(input.AuthenticationKey))
				throw new InvalidAuthenticationKeyException("You supplied an invalid authentication key. Please verify the key and try again.");
		}

		private static void validatePlatformId(ExternalUserStateInputModel input)
		{
			if (string.IsNullOrEmpty(input.PlatformTypeId))
				throw new InvalidPlatformException("Platform id is required");
		}

		private IEnumerable<StateContext> validateUserCode(ExternalUserStateInputModel input)
		{
			var persons = _stateContextLoader.LoadFor(input);
			if (!persons.Any())
				throw new InvalidUserCodeException(string.Format("No person found for SourceId {0} and UserCode {1}", input.SourceId, input.UserCode));
			return persons;
		}

		[InfoLog]
		protected virtual void ProcessInput(ExternalUserStateInputModel input)
		{
			input.UserCode = FixUserCode(input);
			input.StateCode = FixStateCode(input);

			if (input.IsSnapshot && string.IsNullOrEmpty(input.UserCode))
				CloseSnapshot(input);
			else
			{
				var persons = validateUserCode(input);
				//GLHF
				foreach (var person in persons)
					_processor.Process(person);
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
				let state = _stateMapper.StateFor(
					a.BusinessUnitId,
					a.PlatformTypeId,
					a.StateCode,
					null)
				where !state.IsLogOutState
				select a;

			foreach (var agent in agentsNotAlreadyLoggedOut)
			{
				_processor.Process(
					new StateContext(
						input,
						agent.PersonId,
						agent.BusinessUnitId,
						agent.TeamId.GetValueOrDefault(),
						agent.SiteId.GetValueOrDefault(),
						_now,
						_agentStateReadModelUpdater,
						_previousStateInfoLoader
						)
					);
			}
		}

		[InfoLog]
		[TenantScope]
		public virtual void ReloadSchedulesOnNextCheckForActivityChanges(string tenant, Guid personId)
		{
			_cacheInvalidator.InvalidateSchedules(personId);
		}

		[InfoLog]
		[TenantScope]
		public virtual void CheckForActivityChanges(string tenant)
		{
			_activityChangeProcessor.CheckForActivityChanges();
		}

		[TenantScope]
		public virtual void Touch(string tenant)
		{
			_initializor.EnsureTenantInitialized();
		}

	}
}
