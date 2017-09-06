using System;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition
{
	public static class PersonSkillInfrastructure
	{
		public static void AddColumnsToDataTable(DataTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));
			
			table.Columns.Add("skill_date", typeof(DateTime));
			table.Columns.Add("interval_id", typeof(int));
			table.Columns.Add("person_code", typeof(Guid));
			table.Columns.Add("skill_code", typeof(Guid));
			table.Columns.Add("date_from", typeof(DateTime));
			table.Columns.Add("date_to", typeof(DateTime));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("active", typeof(bool));
		}
	}
}
