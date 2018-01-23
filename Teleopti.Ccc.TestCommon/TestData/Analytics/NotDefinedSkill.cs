using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class NotDefinedSkill : ISkillData
	{
		public IEnumerable<DataRow> Rows { get; set; }

		public int FirstSkillId => -1;
		public Guid FirstSkillCode => Guid.Empty;
		public string FirstSkillName => "Not Defined";

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_skill.CreateTable())
			{
				table.AddSkill(FirstSkillId, FirstSkillCode, FirstSkillName, -1, Guid.Empty, "Not Defined", -1, -1);
				Bulk.Insert(connection, table);
				Rows = table.AsEnumerable();
			}
		}
	}
}