using System;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class DenormalizedScheduleMessageBuilder : IDenormalizedScheduleMessageBuilder
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void Build<T>(ScheduleDenormalizeBase message, IScheduleRange range, DateOnlyPeriod realPeriod, Action<T> actionForEachItem) where T : DenormalizedScheduleBase, new()
		{
			foreach (var scheduleDay in range.ScheduledDayCollection(realPeriod))
			{
				var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
				var personPeriod = scheduleDay.Person.Period(date);
				if (personPeriod == null) continue;

				var projection = scheduleDay.ProjectionService().CreateProjection();
				var significantPart = scheduleDay.SignificantPart();
				var result = new T
				             	{
				             		IsInitialLoad = message.SkipDelete,
				             		IsDefaultScenario = range.Scenario.DefaultScenario,
				             		Datasource = message.Datasource,
				             		BusinessUnitId = message.BusinessUnitId,
				             		Timestamp = DateTime.UtcNow,
				             		ScenarioId = message.ScenarioId,
				             		PersonId = message.PersonId,
				             		TeamId = personPeriod.Team.Id.GetValueOrDefault(),
				             		SiteId = personPeriod.Team.Site.Id.GetValueOrDefault(),
				             		Date = date.Date,
				             		Layers = new Collection<DenormalizedScheduleProjectionLayer>(),
				             		WorkTime = projection.WorkTime(),
				             		ContractTime = projection.ContractTime(),
				             		IsWorkday = IsWorkDay(significantPart),
				             	};

				switch (significantPart)
				{
					case SchedulePartView.MainShift:
						var cat = scheduleDay.AssignmentHighZOrder().MainShift.ShiftCategory;
						result.Label = cat.Description.ShortName;
						result.DisplayColor = cat.DisplayColor.ToArgb();
						break;
					case SchedulePartView.FullDayAbsence:
						result.Label = scheduleDay.PersonAbsenceCollection()[0].Layer.Payload.Description.ShortName;
						break;
					case SchedulePartView.DayOff:
						result.Label = scheduleDay.PersonDayOffCollection()[0].DayOff.Description.ShortName;
						break;
				}

				var projectedPeriod = projection.Period();
				if (projectedPeriod != null)
				{
					result.StartDateTime = projectedPeriod.Value.StartDateTime;
					result.EndDateTime = projectedPeriod.Value.EndDateTime;
				}

				foreach (var layer in projection)
				{
					var description = layer.DisplayDescription();
					var contractTime = projection.ContractTime(layer.Period);

					result.Layers.Add(new DenormalizedScheduleProjectionLayer
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

				actionForEachItem(result);
			}
		}

		private static bool IsWorkDay(SchedulePartView significantPart)
		{
			return significantPart == SchedulePartView.MainShift || significantPart == SchedulePartView.Overtime;
		}
	}
}