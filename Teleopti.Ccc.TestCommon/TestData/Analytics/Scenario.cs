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
		private readonly int _businessUnitId;
		private readonly bool _defaultScenario;
		private readonly string _scenarioName;
		private readonly string _businessUnitName;

		public Scenario(int scenarioId, Guid businessUnitCode, bool defaultScenario): this(scenarioId, businessUnitCode, defaultScenario, 1, string.Empty, string.Empty)
		{
		}
		
		public Scenario(int scenarioId, Guid businessUnitCode, bool defaultScenario, int businessUnitId, string scenarioName, string businessUnitName)
		{
			_scenarioId = scenarioId;
			_businessUnitCode = businessUnitCode;
			_defaultScenario = defaultScenario;
			_businessUnitId = businessUnitId;
			_scenarioName = scenarioName;
			_businessUnitName = businessUnitName;
			ScenarioCode = Guid.NewGuid();
		}

		public int ScenarioId => _scenarioId;
		public Guid ScenarioCode { get; }

		public static Scenario DefaultScenarioFor(int scenarioId, Guid businessUnitCode)
		{
			return new Scenario(scenarioId, businessUnitCode, true);
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			var dummyDate = DateTime.Now;
			using (var table = dim_scenario.CreateTable())
			{
				table.AddScenario(_scenarioId, ScenarioCode, _scenarioName, _defaultScenario, _businessUnitId, _businessUnitCode, 
					_businessUnitName, ExistingDatasources.DefaultRaptorDefaultDatasourceId, dummyDate,dummyDate, dummyDate, false);

				Bulk.Insert(connection, table);
			}
		}
	}
}