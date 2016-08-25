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
		private readonly ActivityChangeChecker _activityChangeChecker;
		private readonly IContextLoader _contextLoader;

		public Rta(
			RtaProcessor processor,
			TenantLoader tenantLoader,
			RtaInitializor initializor,
			ActivityChangeChecker activityChangeChecker,
			IContextLoader contextLoader)
		{
			_processor = processor;
			_tenantLoader = tenantLoader;
			_initializor = initializor;
			_activityChangeChecker = activityChangeChecker;
			_contextLoader = contextLoader;
		}
		
		[InfoLog]
		[TenantScope]
		public virtual void SaveStateBatch(BatchInputModel batch)
		{
			validateAuthenticationKey(batch);
			_initializor.EnsureTenantInitialized();
			validatePlatformId(batch);

			_contextLoader.ForBatch(batch, person =>
			{
				_processor.Process(person);
			});
		}

		[InfoLog]
		[TenantScope]
		public virtual void SaveState(StateInputModel input)
		{
			validateAuthenticationKey(input);
			_initializor.EnsureTenantInitialized();
			validatePlatformId(input);

			_contextLoader.For(input, person =>
			{
				_processor.Process(person);
			});
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

		private void validateAuthenticationKey(IValidatable input)
		{
			input.AuthenticationKey = LegacyAuthenticationKey.MakeEncodingSafe(input.AuthenticationKey);
			if (!_tenantLoader.Authenticate(input.AuthenticationKey))
				throw new InvalidAuthenticationKeyException("You supplied an invalid authentication key. Please verify the key and try again.");
		}

		private static void validatePlatformId(IValidatable input)
		{
			if (string.IsNullOrEmpty(input.PlatformTypeId))
				throw new InvalidPlatformException("Platform id is required");
		}
		
		[InfoLog]
		[TenantScope]
		public virtual void CheckForActivityChanges(string tenant)
		{
			_activityChangeChecker.CheckForActivityChanges();
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
