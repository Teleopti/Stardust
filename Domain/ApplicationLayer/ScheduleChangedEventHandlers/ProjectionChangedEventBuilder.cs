using System;
using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ProjectionChangedEventBuilder : IProjectionChangedEventBuilder
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ProjectionChangedEventBuilder));

		public IEnumerable<T> Build<T>(ScheduleChangedEventBase message, IScheduleRange range, DateOnlyPeriod realPeriod) 
			where T : ProjectionChangedEventBase, new()
		{
			if (Logger.IsDebugEnabled)
				Logger.Debug("Building ProjectionChangedEvent(s)");

			foreach (var scheduleDayBatch in range.ScheduledDayCollection(realPeriod).Batch(50))
			{
				var scheduleDays = new List<ProjectionChangedEventScheduleDay>();
				foreach (var scheduleDay in scheduleDayBatch)
				{
					if (Logger.IsDebugEnabled)
						Logger.Debug("Adding a day to ProjectionChangedEvent");

					var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
					var personPeriod = scheduleDay.Person.Period(date);
					if (personPeriod == null)
					{
						if (Logger.IsDebugEnabled)
							Logger.Debug("Person did not have this day in any person period, skipping that day");
						continue;
					}

					var projection = scheduleDay.ProjectionService().CreateProjection();
					
					var significantPart = scheduleDay.SignificantPart();
					if (emptyScheduleOnInitialLoad(message, significantPart))
					{
						if (Logger.IsDebugEnabled)
							Logger.Debug("Skipping this day for reason: ?emptyScheduleOnInitialLoad?");
						continue;
					}

					var eventScheduleDay = new ProjectionChangedEventScheduleDay
						{
							TeamId = personPeriod.Team.Id.GetValueOrDefault(),
							SiteId = personPeriod.Team.Site.Id.GetValueOrDefault(),
							Date = date.Date,
							WorkTime = projection.WorkTime(),
							ContractTime = projection.ContractTime(),
						};
					var layers = new List<ProjectionChangedEventLayer>();

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
							eventScheduleDay.DayOff = new ProjectionChangedEventDayOff
								{
									StartDateTime = scheduleDay.DateOnlyAsPeriod.Period().StartDateTime,
									EndDateTime = scheduleDay.DateOnlyAsPeriod.Period().EndDateTime
								};
							var dayOff = scheduleDay.PersonAssignment().DayOff();
							eventScheduleDay.ShortName = dayOff.Description.ShortName;
							eventScheduleDay.Name = dayOff.Description.Name;
							if (projection.HasLayers)
								eventScheduleDay.IsFullDayAbsence = true;
							break;
						case SchedulePartView.None:
							eventScheduleDay.ShortName = "";
							eventScheduleDay.NotScheduled = true;
							break;
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

						layers.Add(new ProjectionChangedEventLayer
							{
								Name = description.Name,
								ShortName = description.ShortName,
								ContractTime = contractTime,
								PayloadId = layer.Payload.UnderlyingPayload.Id.GetValueOrDefault(),
								IsAbsence = layer.Payload.UnderlyingPayload is IAbsence,
								DisplayColor =
									isPayloadAbsence ? (layer.Payload as IAbsence).DisplayColor.ToArgb() : layer.DisplayColor().ToArgb(),
								RequiresSeat = requiresSeat,
								WorkTime = layer.WorkTime(),
								StartDateTime = layer.Period.StartDateTime,
								EndDateTime = layer.Period.EndDateTime,
								IsAbsenceConfidential = isPayloadAbsence && (layer.Payload as IAbsence).Confidential
							});
					}

					ProjectionChangedEventShift shift = null;

					var projectedPeriod = projection.Period();
					if (projectedPeriod != null || layers.Count > 0)
					{
						shift = new ProjectionChangedEventShift();
						if (projectedPeriod != null)
						{
							shift.StartDateTime = projectedPeriod.Value.StartDateTime;
							shift.EndDateTime = projectedPeriod.Value.EndDateTime;
						}
						shift.Layers = layers;
					}

					eventScheduleDay.Shift = shift;
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
						ScheduleDays = scheduleDays,
						TrackId = message.TrackId
					};
			}
		}

		private static bool emptyScheduleOnInitialLoad(ScheduleChangedEventBase message, SchedulePartView significantPart)
		{
			return message.SkipDelete && significantPart == SchedulePartView.None;
		}

	}
}