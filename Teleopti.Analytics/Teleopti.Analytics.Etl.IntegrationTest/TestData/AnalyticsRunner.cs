using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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
				SqlCommand command = connection.CreateCommand();
				connection.Open();
				string script = "";

				command.CommandType = System.Data.CommandType.Text;
				command.CommandText =
					"IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_setupTestData]') AND type in (N'P', N'PC')) DROP PROCEDURE [mart].[sys_setupTestData]";
				command.ExecuteNonQuery();

				var file = new FileInfo(@"TestData\mart.sys_setupTestData.sql");
				script = file.OpenText().ReadToEnd();
				command.CommandText = script;
				command.ExecuteNonQuery();

				command.CommandType = System.Data.CommandType.Text; 
				command.CommandText =
					"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.Add_QueueAgent_stat') AND type in (N'P', N'PC')) DROP PROCEDURE dbo.Add_QueueAgent_stat";
				command.ExecuteNonQuery();

				file = new FileInfo(@"TestData\dbo.Add_QueueAgent_stat.sql");
				script = file.OpenText().ReadToEnd();
				command.CommandText = script;
				command.ExecuteNonQuery();

				command.CommandText =
					"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_agent_schedule_adherence_for_test]') AND type in (N'P', N'PC')) DROP PROCEDURE [mart].[report_data_agent_schedule_adherence_for_test]";
				command.ExecuteNonQuery();

				file = new FileInfo(@"TestData\mart.report_data_agent_schedule_adherence_for_test.sql");
				script = file.OpenText().ReadToEnd();
				command.CommandText = script;
				command.ExecuteNonQuery();

				command.CommandType = System.Data.CommandType.StoredProcedure;
				command.CommandText = "mart.sys_setupTestData";
				command.ExecuteNonQuery();

				command.CommandText = "dbo.Add_QueueAgent_stat";
				command.Parameters.AddWithValue("@TestDay", DateTime.Today);
				command.Parameters.AddWithValue("@agent_id", 52);
				command.Parameters.AddWithValue("@orig_agent_id", 152);
				command.Parameters.AddWithValue("@agent_name", "Ola H");
				command.ExecuteNonQuery();

			}
		}
	}
}