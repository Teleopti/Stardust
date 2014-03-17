using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Analytics.Etl.IntegrationTest.TestData
{
	public static class AnalyticsRunner
	{
		public static void RunAnalyticsBaseData(IList<IAnalyticsDataSetup> extraSetups)
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			var dates = new CurrentBeforeAndAfterWeekDates();
			var dataSource = new ExistingDatasources(timeZones);
			var intervals = new QuarterOfAnHourInterval();

			analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Setup(new EternityAndNotDefinedDate());
			analyticsDataFactory.Setup(dates);
			analyticsDataFactory.Setup(intervals);
			analyticsDataFactory.Setup(new FillBridgeTimeZoneFromData(dates, intervals, timeZones, dataSource));

			foreach (var extraSetup in extraSetups)
			{
				analyticsDataFactory.Setup(extraSetup);
			}   
			analyticsDataFactory.Persist();
		} 

		public static void RunSysSetupTestData()
		{
			using (var connection = new SqlConnection(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix))
			{
				connection.Open();
				var file = new FileInfo(@"TestData\mart.sys_setupTestData.sql");
				string script = file.OpenText().ReadToEnd();

				new SqlBatchExecutor(connection, new NullLog()).ExecuteBatchSql(script);
				using (var command = new SqlCommand("exec [mart].[sys_setupTestData]", connection))
					command.ExecuteNonQuery();

				file = new FileInfo(@"TestData\dbo.Add_QueueAgent_stat.sql");
				script = file.OpenText().ReadToEnd();

				new SqlBatchExecutor(connection, new NullLog()).ExecuteBatchSql(script);
				using (var command = new SqlCommand(string.Format("exec dbo.Add_QueueAgent_stat @TestDay='{0}', @agent_id={1}, @orig_agent_id='{2}', @agent_name='{3}'", DateTime.Today, 52, "152", "Ola H"), connection))
					command.ExecuteNonQuery();
               
			}
		}
	}
}