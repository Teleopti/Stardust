using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class AWorkload : IAnalyticsDataSetup, IWorkloadData
	{
		private readonly ISkillData _skills;
		private readonly ITimeZoneData _timezones;
		private readonly IBusinessUnitData _businessUnits;
		private readonly IDatasourceData _datasource;

		public int WorkloadId = 0;
		public Guid WorkloadCode = Guid.NewGuid();

		public DataTable Table { get; private set; }

		public AWorkload(ISkillData skills, ITimeZoneData timezones, IBusinessUnitData businessUnits, IDatasourceData datasource)
		{
			_skills = skills;
			_timezones = timezones;
			_businessUnits = businessUnits;
			_datasource = datasource;
		}

		public void Apply(SqlConnection connection, CultureInfo analyticsDataCulture)
		{
			Table = dim_workload.CreateTable();

			Table.AddWorkload(WorkloadId, WorkloadCode, "A workload", _skills.FirstSkillId, _skills.FirstSkillCode, _skills.FirstSkillName, _timezones.UtcTimeZoneId, Guid.NewGuid(), "Forecast method", 1, 1, -1, -1, 0, 1, 1, _businessUnits.BusinessUnitId, _datasource.RaptorDefaultDatasourceId);

		}

	}
}