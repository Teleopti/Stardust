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
		public double ForecastedCalls { get; set; }
		public double ForecastedEmails { get; set; }
		public double ForecastedBackofficeTasks { get; set; }
		public double ForecastedCampaignCalls { get; set; }
		public double ForecastedCallsExclCampaign { get; set; }
		public double ForecastedTalkTimeS { get; set; }
		public double ForecastedCampaignTalkTimeS { get; set; }
		public double ForecastedTalkTimeExclCampaignS { get; set; }
		public double ForecastedAfterCallWorkS { get; set; }
		public double ForecastedCampaignAfterCallWorkS { get; set; }
		public double ForecastedAfterCallWorkExclCampaignS { get; set; }
		public double ForecastedHandlingTimeSeconds { get; set; }
		public double ForecastedCampaignHandlingTimeS { get; set; }
		public double ForecastedHandlingTimeExclCampaignS { get; set; }
		public double PeriodLengthMin { get; set; }
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