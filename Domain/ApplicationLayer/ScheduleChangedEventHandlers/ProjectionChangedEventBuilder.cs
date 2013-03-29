using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ProjectionChangedEventBuilder : IProjectionChangedEventBuilder
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void Build<T>(ScheduleChangedEventBase message, IScheduleRange range, DateOnlyPeriod realPeriod, Action<T> actionForItems) where T : ProjectionChangedEventBase, new()
		{
			foreach (var scheduleDayBatch in range.ScheduledDayCollection(realPeriod).Batch(50))
			{
				var messageList = new List<ProjectionChangedEventScheduleDay>();
				var result = new T
					{
						IsInitialLoad = message.SkipDelete,
						IsDefaultScenario = range.Scenario.DefaultScenario,
						Datasource = message.Datasource,
						BusinessUnitId = message.BusinessUnitId,
						Timestamp = DateTime.UtcNow,
						ScenarioId = message.ScenarioId,
						PersonId = message.PersonId,
						ScheduleDays = messageList
					};
				foreach (var scheduleDay in scheduleDayBatch)
				{
					var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
					var personPeriod = scheduleDay.Person.Period(date);
					if (personPeriod == null) continue;

					var projection = scheduleDay.ProjectionService().CreateProjection();
					var significantPart = scheduleDay.SignificantPart();
					if (significantPart == SchedulePartView.None) continue;

					var eventScheduleDay = new ProjectionChangedEventScheduleDay
						{
							TeamId = personPeriod.Team.Id.GetValueOrDefault(),
							SiteId = personPeriod.Team.Site.Id.GetValueOrDefault(),
							Date = date.Date,
							Layers = new Collection<ProjectionChangedEventLayer>(),
							WorkTime = projection.WorkTime(),
							ContractTime = projection.ContractTime(),
							IsWorkday = isWorkDay(significantPart),
						};

					switch (significantPart)
					{
						case SchedulePartView.MainShift:
							var cat = scheduleDay.AssignmentHighZOrder().MainShift.ShiftCategory;
							eventScheduleDay.Label = cat.Description.ShortName;
							eventScheduleDay.DisplayColor = cat.DisplayColor.ToArgb();
							break;
						case SchedulePartView.FullDayAbsence:
							eventScheduleDay.Label = scheduleDay.PersonAbsenceCollection()[0].Layer.Payload.Description.ShortName;
							break;
						case SchedulePartView.DayOff:
							eventScheduleDay.Label = scheduleDay.PersonDayOffCollection()[0].DayOff.Description.ShortName;
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
						var description = layer.DisplayDescription();
						var contractTime = projection.ContractTime(layer.Period);

						eventScheduleDay.Layers.Add(new ProjectionChangedEventLayer
							{
								Name = description.Name,
								ShortName = description.ShortName,
								ContractTime = contractTime,
								PayloadId = layer.Payload.UnderlyingPayload.Id.GetValueOrDefault(),
								IsAbsence = layer.Payload.UnderlyingPayload is IAbsence,
								DisplayColor = layer.DisplayColor().ToArgb(),
								WorkTime = layer.WorkTime(),
								StartDateTime = layer.Period.StartDateTime,
								EndDateTime = layer.Period.EndDateTime,
							});
					}
					messageList.Add(eventScheduleDay);
				}
				actionForItems(result);
			}
		}

		private static bool isWorkDay(SchedulePartView significantPart)
		{
			return significantPart == SchedulePartView.MainShift || significantPart == SchedulePartView.Overtime;
		}
	}
}