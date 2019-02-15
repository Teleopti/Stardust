using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class ThreeSkills : ISkillData
	{
		private readonly ITimeZoneUtcAndCet _timeZones;
		private readonly IBusinessUnitData _businessUnits;
		private readonly IDatasourceData _datasource;

		public IEnumerable<DataRow> Rows { get; set; }

		public int FirstSkillId { get; set; }
		public Guid FirstSkillCode { get; set; }
		public string FirstSkillName { get; set; }

		public int Skill2Id = 1;
		public Guid Skill2Code = Guid.NewGuid();
		public string Skill2Name = "Skill 2";

		public int Skill3Id = 2;
		public Guid Skill3Code = Guid.NewGuid();
		public string Skill3Name = "Skill 3";

		public ThreeSkills(ITimeZoneUtcAndCet timeZones, IBusinessUnitData businessUnits, IDatasourceData datasource)
		{
			FirstSkillId = 0;
			FirstSkillCode = Guid.NewGuid();
			FirstSkillName = "Skill 1";
			_timeZones = timeZones;
			_businessUnits = businessUnits;
			_datasource = datasource;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			var time_zone_id = _timeZones.UtcTimeZoneId;
			var business_unit_id = _businessUnits.BusinessUnitId;

			using (var table = dim_skill.CreateTable())
			{
				table.AddSkill(FirstSkillId, FirstSkillCode, FirstSkillName, time_zone_id, Guid.NewGuid(), "Forecast method",
					business_unit_id, _datasource.RaptorDefaultDatasourceId);
				table.AddSkill(Skill2Id, Skill2Code, Skill2Name, time_zone_id, Guid.NewGuid(), "Forecast method", business_unit_id,
					_datasource.RaptorDefaultDatasourceId);
				table.AddSkill(Skill3Id, Skill3Code, Skill3Name, time_zone_id, Guid.NewGuid(), "Forecast method", business_unit_id,
					_datasource.RaptorDefaultDatasourceId);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}
	}
}