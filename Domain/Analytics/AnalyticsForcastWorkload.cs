using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsForcastWorkload
	{
		public int DateId { get; set; }
		public int IntervalId { get; set; }
		public DateTime StartTime { get; set; }
		public int WorkloadId { get; set; }
		public int ScenarioId { get; set; }
		public DateTime EndTime { get; set; }
		public int SkillId { get; set; }
		public double ForecastedCalls { get; set; }
		public double ForecastedEmails { get; set; }
		public double ForecastedBackofficeTasks { get; set; }
		public double ForecastedCampaignCalls { get; set; }
		public double ForecastedCallsExclCampaign { get; set; }
		public double ForecastedTalkTimeSeconds { get; set; }
		public double ForecastedCampaignTalkTimeSeconds { get; set; }
		public double ForecastedTalkTimeExclCampaignSeconds { get; set; }
		public double ForecastedAfterCallWorkSeconds { get; set; }
		public double ForecastedCampaignAfterCallWorkSeconds { get; set; }
		public double ForecastedAfterCallWorkExclCampaignSeconds { get; set; }
		public double ForecastedHandlingTimeSeconds { get; set; }
		public double ForecastedCampaignHandlingTimeSeconds { get; set; }
		public double ForecastedHandlingTimeExclCampaignSeconds { get; set; }
		public double PeriodLengthMinutes { get; set; }
		public int BusinessUnitId { get; set; }
		public int DatasourceId { get; set; }
		public DateTime InsertDate { get; set; }
		public DateTime UpdateDate { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }

		public override bool Equals(object obj)
		{
			var workload = obj as AnalyticsForcastWorkload;
			return workload != null &&
				   DateId == workload.DateId &&
				   IntervalId == workload.IntervalId &&
				   StartTime == workload.StartTime &&
				   WorkloadId == workload.WorkloadId &&
				   ScenarioId == workload.ScenarioId;
		}

		public override int GetHashCode()
		{
			var hashCode = 1802931970;
			hashCode = hashCode * -1521134295 + DateId.GetHashCode();
			hashCode = hashCode * -1521134295 + IntervalId.GetHashCode();
			hashCode = hashCode * -1521134295 + StartTime.GetHashCode();
			hashCode = hashCode * -1521134295 + WorkloadId.GetHashCode();
			hashCode = hashCode * -1521134295 + ScenarioId.GetHashCode();
			return hashCode;
		}
	}
}