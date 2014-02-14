using System;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class Scenario : IAnalyticsDataSetup
	{
		private readonly int _scenarioId;
		private readonly Guid _businessUnitCode;
		private readonly bool _defaultScenario;

		private Scenario(int scenarioId, Guid businessUnitCode, bool defaultScenario)
		{
			_scenarioId = scenarioId;
			_businessUnitCode = businessUnitCode;
			_defaultScenario = defaultScenario;
		}

		public static Scenario DefaultScenarioFor(int scenarioId, Guid businessUnitCode)
		{
			return new Scenario(scenarioId, businessUnitCode, true);
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			var dummyDate = DateTime.Now;
			using (var table = dim_scenario.CreateTable())
			{
				table.AddScenario(_scenarioId, Guid.NewGuid(), string.Empty, _defaultScenario, 1, _businessUnitCode, string.Empty, 1, dummyDate, dummyDate, dummyDate, false);

				Bulk.Insert(connection, table);
			}
		}
	}
}