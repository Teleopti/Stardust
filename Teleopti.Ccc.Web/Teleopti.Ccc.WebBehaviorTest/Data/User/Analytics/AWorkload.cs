using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Model;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class AWorkload : IStatisticsDataSetup
	{
		public int WorkloadId = 0;
		public Guid WorkloadCode = Guid.NewGuid();
		public DataTable Table;

		public void Apply(SqlConnection connection, CultureInfo statisticsDataCulture)
		{
			var skillData = UserFactory.User().UserData<ThreeSkills>();
			var timeZoneData = UserFactory.User().UserData<UtcAndCetTimeZones>();
			var businessUnitData = UserFactory.User().UserData<BusinessUnit>();

			Table = dim_workload.CreateTable();

			Table.AddRow(WorkloadId, WorkloadCode, "A workload", skillData.Skill1Id, skillData.Skill1Code, skillData.Skill1Name, timeZoneData.UtcTimeZoneId, Guid.NewGuid(), "Forecast method", 1, 1, -1, -1, 0, 1, 1, businessUnitData.BusinessUnitId, sys_datasource.RaptorDefaultDatasourceId, DateTime.Now, DateTime.Now, DateTime.Now, false);

		}
	}
}