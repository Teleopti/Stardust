using System;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;


namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class QueueStatisticsForSkill : IAnalyticsDataSetup
	{
		private readonly string _skillName;
		private readonly DateTime _latestStatisticsTime;

		public QueueStatisticsForSkill(string skillName, DateTime latestStatisticsTime)
		{
			_skillName = skillName;
			_latestStatisticsTime = latestStatisticsTime;
		}
		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			var intervalData = DefaultAnalyticsDataCreator.GetInterval();
			var timeZoneData = DefaultAnalyticsDataCreator.GetTimeZoneRows();
			var datasourceData = DefaultAnalyticsDataCreator.GetDataSources();

			var theDay = new SpecificDate
			{
				Date = new DateOnly(_latestStatisticsTime),
				DateId = 0,
				Rows = new[] { DefaultAnalyticsDataCreator.GetDateRow(_latestStatisticsTime.Date) }
			};
			var bridgeTimeZone = new FillBridgeTimeZoneFromData(theDay, intervalData, timeZoneData, datasourceData);
			bridgeTimeZone.Apply(connection, userCulture, analyticsDataCulture);
			var day = new Tuple<SpecificDate, IBridgeTimeZone>(theDay, bridgeTimeZone);

			var queue = loadSkillQueue(connection, datasourceData);
			var latestStatisticsIntervalId = new IntervalBase(_latestStatisticsTime, 96).Id;
			new FactQueue(day.Item1, intervalData, queue, datasourceData, day.Item2, latestStatisticsIntervalId)
				.Apply(connection, userCulture, analyticsDataCulture);
		}

		private IQueueData loadSkillQueue(SqlConnection connection, IDatasourceData datasourceData)
		{
			var sql = "select top 1 q.queue_id as QueueId from mart.dim_workload w " +
				"inner join mart.bridge_queue_workload b on w.workload_id = b.workload_id " +
				"inner join mart.dim_queue q on b.queue_id = q.queue_id " +
				"where w.skill_name = '" + _skillName + "'";

			using (var command = new SqlCommand(sql, connection))
			{
				var queueId = (int)command.ExecuteScalar();
				var table = dim_queue.CreateTable();
				table.AddQueue(queueId, -1, "", "", "", "", datasourceData.RaptorDefaultDatasourceId);
				var queue = new AQueue(datasourceData) { Rows = table.Select() };
				return queue;
			}
		}
	}
}