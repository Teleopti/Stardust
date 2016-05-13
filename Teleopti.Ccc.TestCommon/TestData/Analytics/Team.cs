using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class Team : IAnalyticsDataSetup
	{
		public IEnumerable<DataRow> Rows { get; set; }

		private readonly int _teamId;
		private readonly Guid? _teamCode;
		private readonly string _teamName;
		private readonly int _scorecardId;
		private readonly int _businessUnitId;
		private readonly int _siteId;
		private readonly int _datasourceId;

		public Team(int teamId, Guid? teamCode, string teamName, int scorecardId, int businessUnitId, int siteId, int datasourceId)
		{
			_teamId = teamId;
			_teamCode = teamCode;
			_teamName = teamName;
			_scorecardId = scorecardId;
			_datasourceId = datasourceId;
			_businessUnitId = businessUnitId;
			_siteId = siteId;
		}

		public static Team NotDefinedTeam()
		{
			return new Team(-1, null, "Not Defined", -1, -1, -1 , -1);
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_team.CreateTable())
			{
				table.AddTeam(_teamId, _teamCode, _teamName, _scorecardId ,_siteId, _businessUnitId, _datasourceId);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}
	}
}