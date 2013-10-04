using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ProjectionChangedEventBuilder : IProjectionChangedEventBuilder
	{
		public IEnumerable<T> Build<T>(ScheduleChangedEventBase message, IScheduleRange range, DateOnlyPeriod realPeriod) 
			where T : ProjectionChangedEventBase, new()
		{
			foreach (var scheduleDayBatch in range.ScheduledDayCollection(realPeriod).Batch(50))
			{
				var scheduleDays = new List<ProjectionChangedEventScheduleDay>();
				foreach (var scheduleDay in scheduleDayBatch)
				{
					var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
					var personPeriod = scheduleDay.Person.Period(date);
					if (personPeriod == null) continue;

					var projection = scheduleDay.ProjectionService().CreateProjection();
					var significantPart = scheduleDay.SignificantPart();
					if (emptyScheduleOnInitialLoad(message, significantPart)) continue;

					var eventScheduleDay = new ProjectionChangedEventScheduleDay
						{
							TeamId = personPeriod.Team.Id.GetValueOrDefault(),
							SiteId = personPeriod.Team.Site.Id.GetValueOrDefault(),
							Date = date.Date,
							Layers = new Collection<ProjectionChangedEventLayer>(),
							WorkTime = projection.WorkTime(),
							ContractTime = projection.ContractTime(),
						};

					switch (significantPart)
					{
						case SchedulePartView.Overtime:
							eventScheduleDay.IsWorkday = true;
							break;
						case SchedulePartView.MainShift:
							var shiftCategory = scheduleDay.PersonAssignment().ShiftCategory;
							eventScheduleDay.IsWorkday = true;
							eventScheduleDay.ShortName = shiftCategory.Description.ShortName;
							eventScheduleDay.DisplayColor = shiftCategory.DisplayColor.ToArgb();
							break;
						case SchedulePartView.FullDayAbsence:
							eventScheduleDay.IsFullDayAbsence = true;
							eventScheduleDay.ShortName = scheduleDay.PersonAbsenceCollection()[0].Layer.Payload.Description.ShortName;
							break;
						case SchedulePartView.DayOff:
							eventScheduleDay.IsDayOff = true;
							var dayOff = scheduleDay.PersonAssignment().DayOff();
							eventScheduleDay.ShortName = dayOff.Description.ShortName;
							eventScheduleDay.Name = dayOff.Description.Name;
							break;
						case SchedulePartView.None:
							eventScheduleDay.ShortName = "";
							eventScheduleDay.NotScheduled = true;
							break;
					}

					var projectedPeriod = projection.Period();
					if (projectedPeriod != null)
					{
						eventScheduleDay.StartDateTime = projectedPeriod.Value.StartDateTime;
						eventScheduleDay.EndDateTime = projectedPeriod.Value.EndDateTime;
					}

					foreach (var layer in projection)
					{
						var isPayloadAbsence = (layer.Payload is IAbsence);
						var description = isPayloadAbsence
							                  ? (layer.Payload as IAbsence).Description
							                  : layer.DisplayDescription();
						var contractTime = projection.ContractTime(layer.Period);
						var requiresSeat = false;
						var activity = layer.Payload.UnderlyingPayload as IActivity;
						if (activity != null)
						{
							requiresSeat = activity.RequiresSeat;
						}

						eventScheduleDay.Layers.Add(new ProjectionChangedEventLayer
							{
								Name = description.Name,
								ShortName = description.ShortName,
								ContractTime = contractTime,
								PayloadId = layer.Payload.UnderlyingPayload.Id.GetValueOrDefault(),
								IsAbsence = layer.Payload.UnderlyingPayload is IAbsence,
								DisplayColor = isPayloadAbsence ? (layer.Payload as IAbsence).DisplayColor.ToArgb() : layer.DisplayColor().ToArgb(),
								RequiresSeat = requiresSeat,
								WorkTime = layer.WorkTime(),
								StartDateTime = layer.Period.StartDateTime,
								EndDateTime = layer.Period.EndDateTime,
								IsAbsenceConfidential = isPayloadAbsence && (layer.Payload as IAbsence).Confidential
							});
					}
					scheduleDays.Add(eventScheduleDay);
				}
				yield return new T
					{
						IsInitialLoad = message.SkipDelete,
						IsDefaultScenario = range.Scenario.DefaultScenario,
						Datasource = message.Datasource,
						BusinessUnitId = message.BusinessUnitId,
						Timestamp = DateTime.UtcNow,
						ScenarioId = message.ScenarioId,
						PersonId = message.PersonId,
						ScheduleDays = scheduleDays
					};
			}
		}

		private static bool emptyScheduleOnInitialLoad(ScheduleChangedEventBase message, SchedulePartView significantPart)
		{
			return message.SkipDelete && significantPart == SchedulePartView.None;
		}

	}
}