using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
	[EnabledBy(Toggles.RTA_ScheduleProjectionReadOnlyHangfire_35703)]
	public class ScheduleProjectionReadOnlyUpdater :
		ScheduleProjectionReadOnlyUpdaterBase,
		IHandleEvent<ProjectionChangedEventForScheduleProjection>,
		IHandleEvent<ProjectionChangedEvent>,
		IRunOnHangfire
	{
		public ScheduleProjectionReadOnlyUpdater(IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister, IEventPublisher eventPublisher)
			: base(scheduleProjectionReadOnlyPersister, eventPublisher)
		{
		}

		[UnitOfWork]
		public override void Handle(ProjectionChangedEvent @event)
		{
			base.Handle(@event);
		}

		[UnitOfWork]
		public override void Handle(ProjectionChangedEventForScheduleProjection @event)
		{
			base.Handle(@event);
		}
	}

	[DisabledBy(Toggles.RTA_ScheduleProjectionReadOnlyHangfire_35703)]
	public class ScheduleProjectionReadOnlyUpdaterBus:
		ScheduleProjectionReadOnlyUpdaterBase,
		IHandleEvent<ProjectionChangedEventForScheduleProjection>,
		IHandleEvent<ProjectionChangedEvent>,
#pragma warning disable 618
		IRunOnServiceBus
#pragma warning restore 618
	{
		public ScheduleProjectionReadOnlyUpdaterBus(IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister, IEventPublisher eventPublisher) : 
			base(scheduleProjectionReadOnlyPersister, eventPublisher)
		{
		}
	}

	public class ScheduleProjectionReadOnlyUpdaterBase
	{
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
		private readonly IEventPublisher _eventPublisher;

		public ScheduleProjectionReadOnlyUpdaterBase(IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister, IEventPublisher eventPublisher)
		{
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
			_eventPublisher = eventPublisher;
		}

		public virtual void Handle(ProjectionChangedEventForScheduleProjection @event)
		{
			handleProjectionChanged(@event);
		}

		public virtual void Handle(ProjectionChangedEvent @event)
		{
			handleProjectionChanged(@event);
		}

		private void handleProjectionChanged(ProjectionChangedEventBase @event)
		{
			if (!@event.IsDefaultScenario) return;
			
			foreach (var scheduleDay in @event.ScheduleDays)
			{
				var date = new DateOnly(scheduleDay.Date);

				var add = @event.IsInitialLoad ||
						  _scheduleProjectionReadOnlyPersister.BeginAddingSchedule(
							  date,
							  @event.ScenarioId,
							  @event.PersonId,
							  scheduleDay.Version);

				if (!add) continue;
				if (scheduleDay.Shift == null) continue;

				foreach (var layer in scheduleDay.Shift.Layers)
				{
					_scheduleProjectionReadOnlyPersister.AddActivity(
						new ScheduleProjectionReadOnlyModel
						{
							PersonId = @event.PersonId,
							ScenarioId = @event.ScenarioId,
							BelongsToDate = date,
							PayloadId = layer.PayloadId,
							WorkTime = layer.WorkTime,
							ContractTime = layer.ContractTime,
							StartDateTime = layer.StartDateTime,
							EndDateTime = layer.EndDateTime,
							Name = layer.Name,
							ShortName = layer.ShortName,
							DisplayColor = layer.DisplayColor
						});
				}
			}

			_eventPublisher.Publish(new ScheduleProjectionReadModelChangedEvent
			{
				PersonId = @event.PersonId
			});
		}
	}
}
