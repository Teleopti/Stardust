using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class FillFactForecastWorkload : IAnalyticsDataSetup
	{
		private readonly IDateData _date;
		private readonly IIntervalData _intervals;
		private readonly ISkillData _skillData;

		public int ScenarioId { get; set; }
		public int WorkloadId { get; set; }
		public int ForecastedCalls { get; set; }
		public int ForecastedEmails { get; set; }
		public int ForecastedBackofficeTasks { get; set; }
		public int ForecastedCampaignCalls { get; set; }
		public int ForecastedCallsExclCampaign { get; set; }
		public int ForecastedTalkTimeS { get; set; }
		public int ForecastedCampaignTalkTimeS { get; set; }
		public int ForecastedTalkTimeExclCampaignS { get; set; }
		public int ForecastedAfterCallWorkS { get; set; }
		public int ForecastedCampaignAfterCallWorkS { get; set; }
		public int ForecastedAfterCallWorkExclCampaignS { get; set; }
		public int ForecastedHandlingTimeSeconds { get; set; }
		public int ForecastedCampaignHandlingTimeS { get; set; }
		public int ForecastedHandlingTimeExclCampaignS { get; set; }
		public int PeriodLengthMin { get; set; }
		public int BusinessUnitId { get; set; }

		public FillFactForecastWorkload(IDateData date, IIntervalData intervals, ISkillData skillData)
		{
			_date = date;
			_intervals = intervals;
			_skillData = skillData;
		}

		public IEnumerable<DataRow> Rows { get; set; }

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			var table = fact_forecast_workload.CreateTable();
			var dates = _date.Rows.AsEnumerable().Select(x => (int)x["date_id"]).ToList();
			var intervals = _intervals.Rows.AsEnumerable()
				.Select(x => new { intervalId = (int)x["interval_id"], startTime = (DateTime)x["interval_start"], endTime = (DateTime)x["interval_end"]}).ToList();

			foreach (var date in dates)
			{
				foreach (var interval in intervals)
				{
					table.AddForecastWorkload(date, interval.intervalId, interval.startTime, WorkloadId, ScenarioId,
						interval.endTime, _skillData.FirstSkillId, ForecastedCalls, ForecastedEmails, ForecastedBackofficeTasks,
						ForecastedCampaignCalls, ForecastedCallsExclCampaign, ForecastedTalkTimeS, ForecastedCampaignTalkTimeS,
						ForecastedTalkTimeExclCampaignS, ForecastedAfterCallWorkS, ForecastedCampaignAfterCallWorkS,
						ForecastedAfterCallWorkExclCampaignS, ForecastedHandlingTimeSeconds, ForecastedCampaignHandlingTimeS,
						ForecastedHandlingTimeExclCampaignS, PeriodLengthMin, BusinessUnitId);
				}
			}

			Bulk.Insert(connection, table);
			Rows = table.AsEnumerable();
		}
	}
}