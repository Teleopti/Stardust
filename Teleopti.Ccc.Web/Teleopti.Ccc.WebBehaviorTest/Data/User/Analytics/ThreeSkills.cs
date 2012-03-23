using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Sql;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class ThreeSkills : IAnalyticsDataSetup, ISkillData
	{
		private readonly ITimeZoneData _timeZones;
		private readonly IBusinessUnitData _businessUnits;

		public DataTable Table { get; set; }

		public int FirstSkillId { get; set; }
		public Guid FirstSkillCode { get; set; }
		public string FirstSkillName { get; set; }

		public int Skill2Id = 1;
		public Guid Skill2Code = Guid.NewGuid();
		public string Skill2Name = "Skill 2";

		public int Skill3Id = 2;
		public Guid Skill3Code = Guid.NewGuid();
		public string Skill3Name = "Skill 3";

		public ThreeSkills(ITimeZoneData timeZones, IBusinessUnitData businessUnits)
		{
			FirstSkillId = 0;
			FirstSkillCode = Guid.NewGuid();
			FirstSkillName = "Skill 1";
			_timeZones = timeZones;
			_businessUnits = businessUnits;
		}

		public void Apply(SqlConnection connection, CultureInfo statisticsDataCulture)
		{
			var time_zone_id = _timeZones.UtcTimeZoneId;
			var business_unit_id = _businessUnits.BusinessUnitId;

			Table = dim_skill.CreateTable();

			Table.AddRow(FirstSkillId, FirstSkillCode, FirstSkillName, time_zone_id, Guid.NewGuid(), "Forecast method", business_unit_id, sys_datasource.RaptorDefaultDatasourceId);
			Table.AddRow(Skill2Id, Skill2Code, Skill2Name, time_zone_id, Guid.NewGuid(), "Forecast method", business_unit_id, sys_datasource.RaptorDefaultDatasourceId);
			Table.AddRow(Skill3Id, Skill3Code, Skill3Name, time_zone_id, Guid.NewGuid(), "Forecast method", business_unit_id, sys_datasource.RaptorDefaultDatasourceId);

			Bulk.Insert(connection, Table);
		}

	}
}