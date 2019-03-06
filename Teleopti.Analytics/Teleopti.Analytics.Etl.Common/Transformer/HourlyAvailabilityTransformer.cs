using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class HourlyAvailabilityTransformer : IEtlTransformer<IScheduleDay>
	{
		public void Transform(IEnumerable<IScheduleDay> rootList, DataTable table)
		{
			foreach (var schedulePart in rootList)
			{
				IStudentAvailabilityRestriction availRestriction =
					schedulePart.RestrictionCollection().OfType<IStudentAvailabilityRestriction>().FirstOrDefault();

				if (availRestriction == null)
					continue;
				
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
			dataRow["business_unit_code"] = schedulePart.Scenario.GetOrFillWithBusinessUnit_DONTUSE().Id;
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
				minutes = (int)availRestriction.WorkTimeLimitation.EndTime.Value.TotalMinutes;
			}
			return minutes;
		}
	}
}