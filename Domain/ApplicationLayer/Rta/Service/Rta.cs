using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.TimeLogger;
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
					BusinessUnitId = @event.CurrentBusinessUnitId.GetValueOrDefault(),
					SiteId = @event.CurrentSiteId,
					TeamId = @event.CurrentTeamId,
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
		private readonly IBatchExecuteStrategy _batchExecuteStrategy;

		public Rta(
			RtaProcessor processor,
			TenantLoader tenantLoader,
			RtaInitializor initializor,
			ActivityChangeProcessor activityChangeProcessor,
			ContextLoader contextLoader,
			IBatchExecuteStrategy batchExecuteStrategy)
		{
			_processor = processor;
			_tenantLoader = tenantLoader;
			_initializor = initializor;
			_activityChangeProcessor = activityChangeProcessor;
			_contextLoader = contextLoader;
			_batchExecuteStrategy = batchExecuteStrategy;
		}
		
		[InfoLog]
		[TenantScope]
		public virtual void SaveStateBatch(IEnumerable<ExternalUserStateInputModel> inputs)
		{
			var rootInput = inputs.First();
			validateAuthenticationKey(rootInput);
			_initializor.EnsureTenantInitialized();
			validatePlatformId(rootInput);

			var exceptions = new ConcurrentBag<Exception>();
			_batchExecuteStrategy.Execute(inputs, input =>
			{
				try
				{
					SaveStateBatchSingle(input);
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
		protected virtual void SaveStateBatchSingle(ExternalUserStateInputModel input)
		{
			ProcessInput(input);
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

		[InfoLog]
		[TenantScope]
		public virtual void CloseSnapshot(CloseSnapshotInputModel input)
		{
			_contextLoader.ForClosingSnapshot(input.SnapshotId, input.SourceId, context =>
			{
				_processor.Process(context);
			});
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

			var found = false;
			_contextLoader.For(input, person =>
			{
				found = true;
				_processor.Process(person);
			});
			if (!found)
				throw new InvalidUserCodeException(string.Format("No person found for SourceId {0} and UserCode {1}", input.SourceId, input.UserCode));
		}

		protected virtual string FixUserCode(ExternalUserStateInputModel input)
		{
			return input.UserCode.Trim();
		}
		
		protected virtual string FixStateCode(ExternalUserStateInputModel input)
		{
			if (!input.IsLoggedOn)
				return LogOutStateCode;
			if (input.StateCode == null)
				return null;
			var stateCode = input.StateCode.Trim();
			const int stateCodeMaxLength = 25;
			if (stateCode.Length > stateCodeMaxLength)
				return stateCode.Substring(0, stateCodeMaxLength);
			return stateCode;
		}
		
		[InfoLog]
		[TenantScope]
		public virtual void CheckForActivityChanges(string tenant)
		{
			_activityChangeProcessor.CheckForActivityChanges();
		}

		[InfoLog]
		[LogTime]
		[TenantScope]
		public virtual void Touch(string tenant)
		{
			_initializor.EnsureTenantInitialized();
		}

	}
}
