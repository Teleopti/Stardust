﻿using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

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
				
				var server = new Server(new ServerConnection(connection));
				server.ConnectionContext.ExecuteNonQuery(script);
				server.ConnectionContext.ExecuteNonQuery("exec [mart].[sys_setupTestData]");

				file = new FileInfo(@"TestData\dbo.Add_QueueAgent_stat.sql");
				script = file.OpenText().ReadToEnd();

				server.ConnectionContext.ExecuteNonQuery(script);
				server.ConnectionContext.ExecuteNonQuery("exec dbo.Add_QueueAgent_stat");
			}
		}
	}
}