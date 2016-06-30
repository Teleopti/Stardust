using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ProjectionChangedEventBuilder : IProjectionChangedEventBuilder
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ProjectionChangedEventBuilder));

		public IEnumerable<T> Build<T>(ScheduleChangedEventBase message, IScheduleRange range, DateOnlyPeriod realPeriod, IEnumerable<ProjectionVersion> versions)
			where T : ProjectionChangedEventBase, new()
		{
			Logger.Debug("Building ProjectionChangedEvent(s)");

			foreach (var scheduleDayBatch in range.ScheduledDayCollection(realPeriod).Batch(50))
			{
				var scheduleDays = new List<ProjectionChangedEventScheduleDay>();
				foreach (var scheduleDay in scheduleDayBatch)
				{
					Logger.Debug("Adding a day to ProjectionChangedEvent");

					var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
					var version = versions.Single(x => x.Date == date).Version;

					var significantPart = scheduleDay.SignificantPart();
					if (emptyScheduleOnInitialLoad(message, significantPart))
					{
						Logger.Debug("Skipping this day for reason: ?emptyScheduleOnInitialLoad?");
						continue;
					}

					var eventScheduleDay = BuildEventScheduleDay(scheduleDay);
					if (eventScheduleDay != null)
					{
						eventScheduleDay.Version = version;
						scheduleDays.Add(eventScheduleDay);
					}
				}

				yield return new T
				{
					IsInitialLoad = message.SkipDelete,
					IsDefaultScenario = range.Scenario.DefaultScenario,
					LogOnDatasource = message.LogOnDatasource,
					LogOnBusinessUnitId = message.LogOnBusinessUnitId,
					Timestamp = message.Timestamp,
					ScenarioId = message.ScenarioId,
					PersonId = message.PersonId,
					ScheduleDays = scheduleDays,
					CommandId = message.CommandId,

				};
			}
		}

		public ProjectionChangedEventScheduleDay BuildEventScheduleDay(IScheduleDay scheduleDay)
		{
			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var personPeriod = scheduleDay.Person.Period(date);
			if (personPeriod == null)
			{
				Logger.Debug("Person did not have this day in any person period, skipping that day");
				return null;
			}

			var projection = scheduleDay.ProjectionService().CreateProjection();

			var significantPart = scheduleDay.SignificantPart();

			var eventScheduleDay = new ProjectionChangedEventScheduleDay
			{
				TeamId = personPeriod.Team.Id.GetValueOrDefault(),
				SiteId = personPeriod.Team.Site.Id.GetValueOrDefault(),
				Date = date.Date,
				WorkTime = projection.WorkTime(),
				ContractTime = projection.ContractTime(),
				PersonPeriodId = personPeriod.Id.GetValueOrDefault(),
				CheckSum = new ShiftTradeChecksumCalculator(scheduleDay).CalculateChecksum()
			};


			switch (significantPart)
			{
				case SchedulePartView.Overtime:
					eventScheduleDay.IsWorkday = true;
					break;
				case SchedulePartView.MainShift:
					var shiftCategory = scheduleDay.PersonAssignment().ShiftCategory;
					eventScheduleDay.IsWorkday = true;
					eventScheduleDay.ShiftCategoryId = shiftCategory.Id.GetValueOrDefault();
					eventScheduleDay.ShortName = shiftCategory.Description.ShortName;
					eventScheduleDay.DisplayColor = shiftCategory.DisplayColor.ToArgb();
					break;
				case SchedulePartView.FullDayAbsence:
					eventScheduleDay.IsFullDayAbsence = true;
					eventScheduleDay.ShortName = scheduleDay.PersonAbsenceCollection()[0].Layer.Payload.Description.ShortName;
					break;
				case SchedulePartView.DayOff:
					var dayOff = scheduleDay.PersonAssignment().DayOff();
					eventScheduleDay.ShortName = dayOff.Description.ShortName;
					eventScheduleDay.Name = dayOff.Description.Name;
					eventScheduleDay.DayOff = new ProjectionChangedEventDayOff
					{
						StartDateTime = scheduleDay.DateOnlyAsPeriod.Period().StartDateTime,
						EndDateTime = scheduleDay.DateOnlyAsPeriod.Period().EndDateTime,
						Anchor = dayOff.Anchor
					};
					if (projection.HasLayers)
						eventScheduleDay.IsFullDayAbsence = true;
					break;
				default:
					eventScheduleDay.ShortName = "";
					eventScheduleDay.NotScheduled = true;
					break;
			}

			var layers = BuildProjectionChangedEventLayers(projection).ToList();

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
			return eventScheduleDay;
		}

		public IEnumerable<ProjectionChangedEventLayer> BuildProjectionChangedEventLayers(IVisualLayerCollection projection)
		{
			var layers = new List<ProjectionChangedEventLayer>();

			foreach (var layer in projection)
			{
				var isPayloadAbsence = (layer.Payload is IAbsence);
				var description = isPayloadAbsence
					? (layer.Payload as IAbsence).Description
					: layer.DisplayDescription();
				var contractTime = projection.ContractTime(layer.Period);
				var overTime = projection.Overtime(layer.Period);
				var paidTime = projection.PaidTime(layer.Period);
				var workTime = projection.WorkTime(layer.Period);
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
					Overtime = overTime,
					MultiplicatorDefinitionSetId = layer.DefinitionSet != null ? layer.DefinitionSet.Id.GetValueOrDefault() : Guid.Empty,
					PayloadId = layer.Payload.UnderlyingPayload.Id.GetValueOrDefault(),
					IsAbsence = layer.Payload.UnderlyingPayload is IAbsence,
					DisplayColor =
						isPayloadAbsence ? (layer.Payload as IAbsence).DisplayColor.ToArgb() : layer.DisplayColor().ToArgb(),
					RequiresSeat = requiresSeat,
					WorkTime = workTime,
					PaidTime = paidTime,
					StartDateTime = layer.Period.StartDateTime,
					EndDateTime = layer.Period.EndDateTime,
					IsAbsenceConfidential = isPayloadAbsence && (layer.Payload as IAbsence).Confidential
				});
			}
			return layers;
		}

		private static bool emptyScheduleOnInitialLoad(ScheduleChangedEventBase message, SchedulePartView significantPart)
		{
			return message.SkipDelete && significantPart == SchedulePartView.None;
		}

	}
}