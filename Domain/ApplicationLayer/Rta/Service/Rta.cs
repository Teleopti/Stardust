using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Logon.Aspects;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentStateCleaner :
		IRunOnHangfire,
		IHandleEvent<PersonDeletedEvent>,
		IHandleEvent<PersonAssociationChangedEvent>,
		IHandleEvent<PersonPeriodChangedEvent>
	{
		private readonly IAgentStatePersister _persister;

		public AgentStateCleaner(IAgentStatePersister persister)
		{
			_persister = persister;
		}

		[UnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			_persister.Delete(@event.PersonId);
		}

		[UnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{
			if (@event.TeamId.HasValue)
				return;
			_persister.Delete(@event.PersonId);
		}

		[UnitOfWork]
		public virtual void Handle(PersonPeriodChangedEvent @event)
		{
			if (!@event.CurrentTeamId.HasValue)
				return;
			var existing = _persister.Get(@event.PersonId);
			if (existing == null)
			{
				_persister.Persist(new AgentState
				{
					PersonId = @event.PersonId,
					// if the current time is used, behavior tests regarding "details" view fail..
					// .. because the current time later faked to an earlier time...
					// .. and the rta service will see it as the activity starting in the past, since the previous state was later..
					ReceivedTime = "2001-01-01 00:00".Utc()
				});
			}
		}
	}

	public class Rta
	{
		public static string LogOutStateCode = "LOGGED-OFF";
		public static string LogOutBySnapshot = "CCC Logged out";

		private readonly RtaProcessor _processor;
		private readonly TenantLoader _tenantLoader;
		private readonly RtaInitializor _initializor;
		private readonly ActivityChangeProcessor _activityChangeProcessor;
		private readonly ContextLoader _contextLoader;

		public Rta(
			RtaProcessor processor,
			TenantLoader tenantLoader,
			RtaInitializor initializor,
			ActivityChangeProcessor activityChangeProcessor,
			ContextLoader contextLoader)
		{
			_processor = processor;
			_tenantLoader = tenantLoader;
			_initializor = initializor;
			_activityChangeProcessor = activityChangeProcessor;
			_contextLoader = contextLoader;
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
		
		[InfoLog]
		protected virtual void ProcessInput(ExternalUserStateInputModel input)
		{
			input.UserCode = FixUserCode(input);
			input.StateCode = FixStateCode(input);

			if (input.IsSnapshot && string.IsNullOrEmpty(input.UserCode))
				CloseSnapshot(input);
			else
			{
				var found = false;
				_contextLoader.For(input, person =>
				{
					found = true;
					_processor.Process(person);
				});
				if (!found)
					throw new InvalidUserCodeException(string.Format("No person found for SourceId {0} and UserCode {1}", input.SourceId, input.UserCode));
			}
		}
		
		protected virtual string FixUserCode(ExternalUserStateInputModel input)
		{
			return input.UserCode.Trim();
		}
		
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
			input.StateCode = LogOutBySnapshot;
			input.PlatformTypeId = Guid.Empty.ToString();

			_contextLoader.ForClosingSnapshot(input, agent =>
			{
				_processor.Process(agent);
			});
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
