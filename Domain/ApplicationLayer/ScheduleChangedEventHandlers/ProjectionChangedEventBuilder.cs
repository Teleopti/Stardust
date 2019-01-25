using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

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

			var projection = scheduleDay.ProjectionService().CreateProjection();

			var significantPart = scheduleDay.SignificantPart();

			var eventScheduleDay = new ProjectionChangedEventScheduleDay
			{
				TeamId = personPeriod?.Team.Id ?? Guid.Empty,
				SiteId = personPeriod?.Team.Site.Id ?? Guid.Empty,
				Date = date.Date,
				WorkTime = projection.WorkTime(),
				ContractTime = projection.ContractTime(),
				PersonPeriodId = personPeriod?.Id ?? Guid.Empty,
				CheckSum = new ShiftTradeChecksumCalculator(scheduleDay).CalculateChecksum()
			};

			eventScheduleDay.IsWorkday = scheduleDay.IsWorkday();
			switch (significantPart)
			{
				case SchedulePartView.Overtime:
					break;
				case SchedulePartView.MainShift:
					var shiftCategory = scheduleDay.PersonAssignment().ShiftCategory;
					eventScheduleDay.ShiftCategoryId = shiftCategory.Id.GetValueOrDefault();
					eventScheduleDay.ShortName = shiftCategory.Description.ShortName;
					eventScheduleDay.DisplayColor = shiftCategory.DisplayColor.ToArgb();
					break;
				case SchedulePartView.FullDayAbsence:
					eventScheduleDay.IsFullDayAbsence = true;
					var absenceCollection = scheduleDay.PersonAbsenceCollection();
					if (absenceCollection.Length > 1) {
						absenceCollection = absenceCollection.OrderBy(a => a.Layer.Payload.Priority)
						.ThenByDescending(a => absenceCollection.IndexOf(a)).ToArray();
					}
					eventScheduleDay.ShortName = absenceCollection.First().Layer.Payload.Description.ShortName;
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
					if (projection.HasLayers && projection.Overtime().Equals(TimeSpan.Zero))
						eventScheduleDay.IsFullDayAbsence = true;
					break;
				default:
					eventScheduleDay.ShortName = "";
					eventScheduleDay.NotScheduled = true;
					break;
			}

			var layers = BuildProjectionChangedEventLayers(projection, scheduleDay.Person).ToList();

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

		public IEnumerable<ProjectionChangedEventLayer> BuildProjectionChangedEventLayers(IVisualLayerCollection projection, IPerson person)
		{
			var layers = new List<ProjectionChangedEventLayer>();

			foreach (var layer in projection)
			{
				IAbsence absence = null;
				if (layer.Payload is IAbsence absencePayload)
				{
					absence = absencePayload;
				}

				var description = absence?.Description ?? layer.Payload.ConfidentialDescription_DONTUSE(person);
				var contractTime = projection.ContractTime(layer.Period);
				var overTime = projection.Overtime(layer.Period);
				var paidTime = projection.PaidTime(layer.Period);
				var workTime = projection.WorkTime(layer.Period);
				var requiresSeat = false;
				if (layer.Payload.UnderlyingPayload is IActivity activity)
				{
					requiresSeat = activity.RequiresSeat;
				}

				layers.Add(new ProjectionChangedEventLayer
				{
					Name = description.Name,
					ShortName = description.ShortName,
					ContractTime = contractTime,
					Overtime = overTime,
					MultiplicatorDefinitionSetId = layer.DefinitionSet?.Id ?? Guid.Empty,
					PayloadId = layer.Payload.UnderlyingPayload.Id.GetValueOrDefault(),
					IsAbsence = layer.Payload.UnderlyingPayload is IAbsence,
					DisplayColor = absence?.DisplayColor.ToArgb() ?? layer.Payload.ConfidentialDisplayColor_DONTUSE(person).ToArgb(),
					RequiresSeat = requiresSeat,
					WorkTime = workTime,
					PaidTime = paidTime,
					StartDateTime = layer.Period.StartDateTime,
					EndDateTime = layer.Period.EndDateTime,
					IsAbsenceConfidential = absence?.Confidential ?? false
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