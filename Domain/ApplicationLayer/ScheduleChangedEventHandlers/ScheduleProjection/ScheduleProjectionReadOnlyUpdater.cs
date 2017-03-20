using System.Reflection;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
	public class ScheduleProjectionReadOnlyUpdater :
		IHandleEvent<ProjectionChangedEventForScheduleProjection>,
		IHandleEvent<ProjectionChangedEvent>,
		IRunOnHangfire
	{
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
		private readonly ILog logger = LogManager.GetLogger(typeof(ScheduleProjectionReadOnlyUpdater));

		public ScheduleProjectionReadOnlyUpdater(IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister)
		{
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
		}

		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEvent @event)
		{
			handleProjectionChanged(@event);
		}

		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEventForScheduleProjection @event)
		{
			handleProjectionChanged(@event);
		}

		private void handleProjectionChanged(ProjectionChangedEventBase @event)
		{
			if (!@event.IsDefaultScenario) return;

			foreach (var scheduleDay in @event.ScheduleDays)
			{
				var date = new DateOnly(scheduleDay.Date);

				try
				{
					var add = @event.IsInitialLoad ||
						  _scheduleProjectionReadOnlyPersister.BeginAddingSchedule(
							  date,
							  @event.ScenarioId,
							  @event.PersonId,
							  scheduleDay.Version);

					if (!add) continue;
				}
				catch (TargetInvocationException exception)
				{
					logger.Error("Cannot add Schedule!", exception);
				}

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
		}
	}
	
}
