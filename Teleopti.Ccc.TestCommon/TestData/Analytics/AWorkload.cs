using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class AWorkload : IWorkloadData
	{
		private readonly ISkillData _skills;
		private readonly ITimeZoneUtcAndCet _timezones;
		private readonly IBusinessUnitData _businessUnits;
		private readonly IDatasourceData _datasource;

		public int WorkloadId = 0;
		public bool IsDeleted { get; set; }
		public Guid WorkloadCode = Guid.NewGuid();

		public IEnumerable<DataRow> Rows { get; set; }

		public AWorkload(ISkillData skills, ITimeZoneUtcAndCet timezones, IBusinessUnitData businessUnits, IDatasourceData datasource)
		{
			_skills = skills;
			_timezones = timezones;
			_businessUnits = businessUnits;
			_datasource = datasource;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_workload.CreateTable())
			{
				table.AddWorkload(WorkloadId, WorkloadCode, "A workload", _skills.FirstSkillId, _skills.FirstSkillCode, _skills.FirstSkillName, _timezones.UtcTimeZoneId, Guid.NewGuid(), "Forecast method", 1, 1, -1, -1, 0, 1, 1, _businessUnits.BusinessUnitId, _datasource.RaptorDefaultDatasourceId, IsDeleted);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}
	}
}