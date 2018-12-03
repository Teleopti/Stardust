using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class IntradayHourlyAvailabilityTransformer : IIntradayAvailabilityTransformer
	{
		// copied old entity impl to be used in list here in ETL to keep old behavior
		private class uniqueStudentAvailabilityDayPerDateAndPerson : IEqualityComparer<IStudentAvailabilityDay>
		{
			public bool Equals(IStudentAvailabilityDay x, IStudentAvailabilityDay y)
			{
				return x.Person.Equals(y.Person) && x.RestrictionDate == y.RestrictionDate;
			}

			public int GetHashCode(IStudentAvailabilityDay obj)
			{
				return obj.Person.GetHashCode() ^ obj.RestrictionDate.GetHashCode();
			}
		}

		public HashSet<IStudentAvailabilityDay> CreateStudentAvailabilityDaySetWithPersonDateEqualitity()
		{
			return new HashSet<IStudentAvailabilityDay>(new uniqueStudentAvailabilityDayPerDateAndPerson());
		}

		public void Transform(IEnumerable<IStudentAvailabilityDay> rootList, DataTable table, ICommonStateHolder stateHolder, IScenario scenario)
		{
			var uniqueDays = CreateStudentAvailabilityDaySetWithPersonDateEqualitity();
			uniqueDays.UnionWith(rootList);

			var dictionary = stateHolder.GetSchedules(uniqueDays, scenario);
			foreach (var availDay in uniqueDays)
			{
				var availRestriction = availDay.RestrictionCollection.FirstOrDefault();
				if (availRestriction == null)
					continue;
				var persons = stateHolder.PersonsWithIds(new List<Guid> { availDay.Person.Id.GetValueOrDefault() });
				if (!persons.Any())
					continue;
				var schedulePart = getScheduleDay(dictionary, availDay.RestrictionDate, persons[0]);
				if (schedulePart == null)
					continue;
				var newDataRow = table.NewRow();
				newDataRow = fillDataRow(newDataRow, availRestriction, schedulePart);
				table.Rows.Add(newDataRow);
			}
		}

		private IScheduleDay getScheduleDay(IDictionary<DateOnly, IScheduleDictionary> dictionary, DateOnly dateOnly,
			IPerson person)
		{
			IScheduleDictionary days;
			if (dictionary.TryGetValue(dateOnly,out days))
			{
				IScheduleRange range;
				if (days.TryGetValue(person, out range))
					return range.ScheduledDay(dateOnly);
			}

			return null;
		}
		private DataRow fillDataRow(DataRow dataRow, IStudentAvailabilityRestriction availRestriction, IScheduleDay schedulePart)
		{
			var availDay = (IStudentAvailabilityDay)availRestriction.Parent;
			dataRow["restriction_date"] = availDay.RestrictionDate.Date;
			dataRow["person_code"] = schedulePart.Person.Id;
			dataRow["scenario_code"] = schedulePart.Scenario.Id;
			dataRow["business_unit_code"] = schedulePart.Scenario.BusinessUnit.Id;
			dataRow["available_time_m"] = getMaxAvailable(availRestriction);
			var workTime = scheduledWorkTime(schedulePart);
			dataRow["scheduled_time_m"] = workTime;
			dataRow["scheduled"] = workTime > 0;
			dataRow["datasource_id"] = 1;

			return dataRow;
		}

		private static int scheduledWorkTime(IScheduleDay scheduleDay)
		{
			var minutes = 0;
			if (scheduleDay.IsScheduled())
			{
				var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();

				if (visualLayerCollection.HasLayers)
				{
					minutes = (int)visualLayerCollection.WorkTime().TotalMinutes;
				}
			}
			return minutes;
		}

		private static int getMaxAvailable(IStudentAvailabilityRestriction availRestriction)
		{
			var start = TimeSpan.FromMinutes(0);
			var end = TimeSpan.FromHours(24);
			if (availRestriction.StartTimeLimitation.StartTime.HasValue)
				start = availRestriction.StartTimeLimitation.StartTime.GetValueOrDefault();

			if (availRestriction.EndTimeLimitation.EndTime.HasValue)
				end = availRestriction.EndTimeLimitation.EndTime.GetValueOrDefault();

			var minutes = (int)end.Add(-start).TotalMinutes;

			if (availRestriction.WorkTimeLimitation.EndTime.HasValue)
			{
				minutes = availRestriction.WorkTimeLimitation.EndTime.Value.Minutes;
			}
			return minutes;
		}
	}
}