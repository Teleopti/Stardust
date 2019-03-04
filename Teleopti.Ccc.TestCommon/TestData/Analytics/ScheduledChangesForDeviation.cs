using System;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class ScheduledChangesForDeviation : IAnalyticsDataSetup
	{
		private readonly Guid _personCode;
		private readonly DateTime _shiftStartLocalDate;
		private readonly Guid _scenarioCode;
		private readonly int _dataSourceId;
		private readonly Guid _businessCode;

		public ScheduledChangesForDeviation(Guid personCode, DateTime shiftStartLocalDate, Guid scenarioCode, int dataSourceId, Guid businessCode)
		{
			_personCode = personCode;
			_shiftStartLocalDate = shiftStartLocalDate;
			_scenarioCode = scenarioCode;
			_dataSourceId = dataSourceId;
			_businessCode = businessCode;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = stg_schedule_changed_servicebus.CreateTable())
			{
				table.AddScheduleChange(_personCode, _shiftStartLocalDate, _scenarioCode, _dataSourceId, _businessCode); 
				Bulk.Insert(connection, table);
			}

		}
	}
}