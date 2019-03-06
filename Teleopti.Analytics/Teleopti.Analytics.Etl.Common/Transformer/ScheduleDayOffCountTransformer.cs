using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class ScheduleDayOffCountTransformer : IScheduleDayOffCountTransformer
	{
		private DataTable _table;

		public void Transform(IEnumerable<IScheduleDay> rootList, DataTable table, int intervalsPerDay)
		{
			_table = table;

			foreach (DataRow dataRow in CreateDataRows(rootList, _table))
			{
				_table.Rows.Add(dataRow);
			}
		}

		public static IList<DataRow> CreateDataRows(IEnumerable<IScheduleDay> schedulePartCollection, DataTable dataTable)
		{
			IList<DataRow> dataRowCollection = new List<DataRow>();

			foreach (IScheduleDay schedulePart in schedulePartCollection)
			{
				if (doDayOffExistInSchedulePart(schedulePart))
				{
					dataRowCollection.Add(CreateDataRow(schedulePart, dataTable));
				}
			}

			return dataRowCollection;
		}

		public static DataRow CreateDataRow(IScheduleDay schedulePart, DataTable dataTable)
		{
			if (dataTable == null)
				return null;
			DataRow dataRow = dataTable.NewRow();
			if (schedulePart == null)
				return dataRow;

			var personDayOff = extractDayOff(schedulePart);
			var ass = schedulePart.PersonAssignment();
			dataRow["schedule_date_local"] = schedulePart.DateOnlyAsPeriod.DateOnly.Date;
			dataRow["person_code"] = schedulePart.Person.Id;
			dataRow["scenario_code"] = schedulePart.Scenario.Id;
			dataRow["starttime"] = personDayOff.Anchor;
			dataRow["day_off_code"] = personDayOff.DayOffTemplateId;
			dataRow["day_off_name"] = personDayOff.Description.Name; //Get from domain
			dataRow["day_off_shortname"] = personDayOff.Description.ShortName; //Get from domain
			dataRow["day_count"] = 1;
			dataRow["business_unit_code"] = schedulePart.Scenario.GetOrFillWithBusinessUnit_DONTUSE().Id;
			dataRow["datasource_update_date"] = ass == null ? DateTime.UtcNow : ass.UpdatedOn;

			return dataRow;
		}

		private static bool doDayOffExistInSchedulePart(IScheduleDay schedulePart)
		{

			SchedulePartView significant = schedulePart.SignificantPart();
			if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
			{
				return true;
			}

			return false;
		}

		private static DayOff extractDayOff(IScheduleDay scheduleDay)
		{
			if (scheduleDay.SignificantPart() == SchedulePartView.ContractDayOff)
			{
				IDayOffTemplate template = new DayOffTemplate(new Description("ContractDayOff", "CD"));
				template.Anchor = TimeSpan.FromHours(12);
				var tempAss = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, scheduleDay.DateOnlyAsPeriod.DateOnly);
				tempAss.SetDayOff(template);
				return tempAss.DayOff();
			}

			return scheduleDay.PersonAssignment().DayOff();
		}
	}
}
