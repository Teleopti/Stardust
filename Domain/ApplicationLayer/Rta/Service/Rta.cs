﻿using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentStateMaintainer :
		IRunOnHangfire,
		IHandleEvent<PersonDeletedEvent>,
		IHandleEvent<PersonAssociationChangedEvent>,
		IHandleEvent<ScheduleProjectionReadOnlyChangedEvent>,
		IHandleEvent<ScheduleChangedEvent>,
		IHandleEventOnQueue<ScheduleChangedEvent>
	{
		private readonly IAgentStatePersister _persister;
		private readonly INow _now;

		public AgentStateMaintainer(IAgentStatePersister persister, INow now)
		{
			_persister = persister;
			_now = now;
		}

		[UnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			_persister.Delete(@event.PersonId, DeadLockVictim.Yes);
		}

		[UnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{
			if (!@event.TeamId.HasValue)
			{
				_persister.Delete(@event.PersonId, DeadLockVictim.Yes);
				return;
			}
			if (@event.ExternalLogons.IsNullOrEmpty())
			{
				_persister.Delete(@event.PersonId, DeadLockVictim.Yes);
				return;
			}

			_persister.Prepare(new AgentStatePrepare
			{
				PersonId = @event.PersonId,
				BusinessUnitId = @event.BusinessUnitId.GetValueOrDefault(),
				SiteId = @event.SiteId,
				TeamId = @event.TeamId,
				ExternalLogons = @event.ExternalLogons.Select(x => new ExternalLogon
				{
					DataSourceId = x.DataSourceId,
					UserCode = x.UserCode,
				}).ToArray()
			}, DeadLockVictim.Yes);
		}

		[UnitOfWork]
		[DisabledBy(Toggles.RTA_FasterUpdateOfScheduleChanges_40536)]
		public virtual void Handle(ScheduleProjectionReadOnlyChangedEvent @event)
		{
			_persister.InvalidateSchedules(@event.PersonId, DeadLockVictim.Yes);
		}

		[UnitOfWork]
		[EnabledBy(Toggles.RTA_FasterUpdateOfScheduleChanges_40536)]
		public virtual void Handle(ScheduleChangedEvent @event)
		{
			if (scheduleChangeToday(@event))
				_persister.InvalidateSchedules(@event.PersonId, DeadLockVictim.Yes);
		}

		public string QueueTo(ScheduleChangedEvent @event)
		{
			return scheduleChangeToday(@event) ? Queues.ScheduleChangesToday : null;
		}

		private bool scheduleChangeToday(ScheduleChangedEvent @event)
		{
			var now = _now.UtcDateTime();
			return new DateTimePeriod(@event.StartDateTime, @event.EndDateTime)
				.Intersect(new DateTimePeriod(now.AddDays(-2), now.AddDays(2)));
		}
	}

	public class Rta
	{
		public static string LogOutStateCode = "LOGGED-OFF";
		public static string LogOutBySnapshot = "CCC Logged out";

		private readonly RtaProcessor _processor;
		private readonly TenantLoader _tenantLoader;
		private readonly ActivityChangeChecker _activityChangeChecker;
		private readonly IContextLoader _contextLoader;

		public Rta(
			RtaProcessor processor,
			TenantLoader tenantLoader,
			ActivityChangeChecker activityChangeChecker,
			IContextLoader contextLoader)
		{
			_processor = processor;
			_tenantLoader = tenantLoader;
			_activityChangeChecker = activityChangeChecker;
			_contextLoader = contextLoader;
		}
		
		[LogInfo]
		[TenantScope]
		public virtual void SaveStateBatch(BatchInputModel batch)
		{
			validateAuthenticationKey(batch);
			validatePlatformId(batch);

			_contextLoader.ForBatch(batch, person =>
			{
				_processor.Process(person);
			});
		}

		[LogInfo]
		[TenantScope]
		public virtual void SaveState(StateInputModel input)
		{
			validateAuthenticationKey(input);
			validatePlatformId(input);

			_contextLoader.For(input, person =>
			{
				_processor.Process(person);
			});
		}
		
		[LogInfo]
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
		
		[LogInfo]
		[TenantScope]
		public virtual void CheckForActivityChanges(string tenant)
		{
			_activityChangeChecker.CheckForActivityChanges();
		}
	}
}
