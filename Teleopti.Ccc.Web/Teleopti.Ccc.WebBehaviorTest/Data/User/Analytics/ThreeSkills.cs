using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Model;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Sql;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class ThreeSkills : IStatisticsDataSetup
	{
		public DataTable Table;

		public void Apply(SqlConnection connection, CultureInfo statisticsDataCulture)
		{
			var time_zone_id = (int) UserFactory.User().UserData<UtcAndCetTimeZones>().Table.Rows[0]["time_zone_id"];
			var business_unit_id = (int)UserFactory.User().UserData<BusinessUnit>().Table.Rows[0]["business_unit_id"];

			Table = dim_skill.CreateTable();

			Table.AddRow(0, Guid.NewGuid(), "Skill 1", time_zone_id, Guid.NewGuid(), "Forecast method", business_unit_id, sys_datasource.RaptorDefaultDatasourceId, DateTime.Now, DateTime.Now, DateTime.Now, false);
			Table.AddRow(1, Guid.NewGuid(), "Skill 2", time_zone_id, Guid.NewGuid(), "Forecast method", business_unit_id, sys_datasource.RaptorDefaultDatasourceId, DateTime.Now, DateTime.Now, DateTime.Now, false);
			Table.AddRow(2, Guid.NewGuid(), "Skill 3", time_zone_id, Guid.NewGuid(), "Forecast method", business_unit_id, sys_datasource.RaptorDefaultDatasourceId, DateTime.Now, DateTime.Now, DateTime.Now, false);

			Bulk.Insert(connection, Table);
		}
	}
}