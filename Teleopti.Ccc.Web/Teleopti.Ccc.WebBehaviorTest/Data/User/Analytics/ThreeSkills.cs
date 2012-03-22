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
		public int Skill1Id = 0;
		public Guid Skill1Code = Guid.NewGuid();
		public string Skill1Name = "Skill 1";

		public int Skill2Id = 1;
		public Guid Skill2Code = Guid.NewGuid();
		public string Skill2Name = "Skill 2";

		public int Skill3Id = 2;
		public Guid Skill3Code = Guid.NewGuid();
		public string Skill3Name = "Skill 3";

		public DataTable Table;

		public void Apply(SqlConnection connection, CultureInfo statisticsDataCulture)
		{
			var time_zone_id = UserFactory.User().UserData<UtcAndCetTimeZones>().UtcTimeZoneId;
			var business_unit_id = UserFactory.User().UserData<BusinessUnit>().BusinessUnitId;

			Table = dim_skill.CreateTable();

			Table.AddRow(Skill1Id, Skill1Code, Skill1Name, time_zone_id, Guid.NewGuid(), "Forecast method", business_unit_id, sys_datasource.RaptorDefaultDatasourceId, DateTime.Now, DateTime.Now, DateTime.Now, false);
			Table.AddRow(Skill2Id, Skill2Code, Skill2Name, time_zone_id, Guid.NewGuid(), "Forecast method", business_unit_id, sys_datasource.RaptorDefaultDatasourceId, DateTime.Now, DateTime.Now, DateTime.Now, false);
			Table.AddRow(Skill3Id, Skill3Code, Skill3Name, time_zone_id, Guid.NewGuid(), "Forecast method", business_unit_id, sys_datasource.RaptorDefaultDatasourceId, DateTime.Now, DateTime.Now, DateTime.Now, false);

			Bulk.Insert(connection, Table);
		}
	}
}