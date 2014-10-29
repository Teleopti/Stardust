﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer
{
	public class IntradayHourlyAvailabilityTransformer : IIntradayAvailabilityTransformer
	{
		public void Transform(IEnumerable<IStudentAvailabilityDay> rootList, DataTable table, ICommonStateHolder stateHolder, IScenario scenario)
		{
			var uniqueDays = new HashSet<IStudentAvailabilityDay>();
			uniqueDays.UnionWith(rootList);
			foreach (var availDay in uniqueDays)
			{
				var availRestriction = availDay.RestrictionCollection.FirstOrDefault();
				if(availRestriction == null)
					continue;
				var persons = stateHolder.PersonsWithIds(new List<Guid> { availDay.Person.Id.GetValueOrDefault() });
				var schedulePart = stateHolder.GetSchedulePartOnPersonAndDate(persons[0], availDay.RestrictionDate, scenario);
				var newDataRow = table.NewRow();
				newDataRow = fillDataRow(newDataRow, availRestriction, schedulePart);
				table.Rows.Add(newDataRow);
				
			}
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