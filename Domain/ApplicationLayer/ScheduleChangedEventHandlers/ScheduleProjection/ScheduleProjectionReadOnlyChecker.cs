using System.Linq;
using System.Reflection;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
	public class ScheduleProjectionReadOnlyChecker
	{
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
		private readonly ILog logger = LogManager.GetLogger(typeof(ScheduleProjectionReadOnlyChecker));
		
		public ScheduleProjectionReadOnlyChecker(IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister)
		{
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
		}
		
		public void Execute(ProjectionChangedEventBase @event)
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

				_scheduleProjectionReadOnlyPersister.AddActivity(
					scheduleDay.Shift.Layers.Select(layer => new ScheduleProjectionReadOnlyModel
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
					}));
			}
		}
	}
}