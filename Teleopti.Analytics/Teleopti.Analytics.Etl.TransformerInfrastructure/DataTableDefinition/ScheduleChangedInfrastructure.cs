using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.ReadModel;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition
{
	public static class ScheduleChangedInfrastructure
    {
        public static void AddColumnsToDataTable(DataTable table)
        {
            table.Columns.Add("schedule_date", typeof(DateTime));
            table.Columns.Add("person_code", typeof(Guid));
            table.Columns.Add("scenario_code", typeof(Guid));
			table.Columns.Add("business_unit_code", typeof(Guid));
			table.Columns.Add("datasource_id", typeof(int));
            table.Columns.Add("insert_date", typeof(DateTime));
            table.Columns.Add("update_date", typeof(DateTime));
            table.Columns.Add("datasource_update_date", typeof(DateTime));
        }

		public static void AddRows(DataTable dataTable, IList<IScheduleChangedReadModel> changed, IScenario scenario, IBusinessUnit currentBusinessUnit)
		{
			foreach (var scheduleChangedReadModel in changed)
			{
				var row = dataTable.NewRow();
				row["schedule_date"] = scheduleChangedReadModel.Date;
				row["person_code"] = scheduleChangedReadModel.Person;
				row["scenario_code"] = scenario.Id;
				row["business_unit_code"] = currentBusinessUnit.Id;
				row["datasource_update_date"] = DateTime.UtcNow;
				dataTable.Rows.Add(row);
			}
		}
    }
}
