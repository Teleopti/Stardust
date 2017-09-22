using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsForecastWorkloadRepository : IAnalyticsForecastWorkloadRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsForecastWorkloadRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}
		public void AddOrUpdate(AnalyticsForcastWorkload analyticsForcastWorkload)
		{
			_analyticsUnitOfWork.Current().Session().CreateSQLQuery($@"
				exec mart.[etl_fact_forecast_workload_add_or_update]
					@date_id=:{nameof(analyticsForcastWorkload.DateId)}
					,@interval_id=:{nameof(analyticsForcastWorkload.IntervalId)}
					,@start_time=:{nameof(analyticsForcastWorkload.StartTime)}
					,@workload_id=:{nameof(analyticsForcastWorkload.WorkloadId)}
					,@scenario_id=:{nameof(analyticsForcastWorkload.ScenarioId)}
					,@end_time=:{nameof(analyticsForcastWorkload.EndTime)}
					,@skill_id=:{nameof(analyticsForcastWorkload.SkillId)}
					,@forecasted_calls=:{nameof(analyticsForcastWorkload.ForecastedCalls)}
					,@forecasted_emails=:{nameof(analyticsForcastWorkload.ForecastedEmails)}
					,@forecasted_backoffice_tasks=:{nameof(analyticsForcastWorkload.ForecastedBackofficeTasks)}
					,@forecasted_campaign_calls=:{nameof(analyticsForcastWorkload.ForecastedCampaignCalls)}
					,@forecasted_calls_excl_campaign=:{nameof(analyticsForcastWorkload.ForecastedCallsExclCampaign)}
					,@forecasted_talk_time_s=:{nameof(analyticsForcastWorkload.ForecastedTalkTimeSeconds)}
					,@forecasted_campaign_talk_time_s=:{nameof(analyticsForcastWorkload.ForecastedCampaignTalkTimeSeconds)}
					,@forecasted_talk_time_excl_campaign_s=:{nameof(analyticsForcastWorkload.ForecastedTalkTimeExclCampaignSeconds)}
					,@forecasted_after_call_work_s=:{nameof(analyticsForcastWorkload.ForecastedAfterCallWorkSeconds)}
					,@forecasted_campaign_after_call_work_s=:{nameof(analyticsForcastWorkload.ForecastedCampaignAfterCallWorkSeconds)}
					,@forecasted_after_call_work_excl_campaign_s=:{nameof(analyticsForcastWorkload.ForecastedAfterCallWorkExclCampaignSeconds)}
					,@forecasted_handling_time_s=:{nameof(analyticsForcastWorkload.ForecastedHandlingTimeSeconds)}
					,@forecasted_campaign_handling_time_s=:{nameof(analyticsForcastWorkload.ForecastedCampaignHandlingTimeSeconds)}
					,@forecasted_handling_time_excl_campaign_s=:{nameof(analyticsForcastWorkload.ForecastedHandlingTimeExclCampaignSeconds)}
					,@period_length_min=:{nameof(analyticsForcastWorkload.PeriodLengthMinutes)}
					,@business_unit_id=:{nameof(analyticsForcastWorkload.BusinessUnitId)}
					,@datasource_update_date=:{nameof(analyticsForcastWorkload.DatasourceUpdateDate)}
			")
			.SetParameter(nameof(analyticsForcastWorkload.DateId), analyticsForcastWorkload.DateId)
			.SetParameter(nameof(analyticsForcastWorkload.IntervalId), analyticsForcastWorkload.IntervalId)
			.SetParameter(nameof(analyticsForcastWorkload.StartTime), analyticsForcastWorkload.StartTime)
			.SetParameter(nameof(analyticsForcastWorkload.WorkloadId), analyticsForcastWorkload.WorkloadId)
			.SetParameter(nameof(analyticsForcastWorkload.ScenarioId), analyticsForcastWorkload.ScenarioId)
			.SetParameter(nameof(analyticsForcastWorkload.EndTime), analyticsForcastWorkload.EndTime)
			.SetParameter(nameof(analyticsForcastWorkload.SkillId), analyticsForcastWorkload.SkillId)
			.SetParameter(nameof(analyticsForcastWorkload.ForecastedCalls), analyticsForcastWorkload.ForecastedCalls)
			.SetParameter(nameof(analyticsForcastWorkload.ForecastedEmails), analyticsForcastWorkload.ForecastedEmails)
			.SetParameter(nameof(analyticsForcastWorkload.ForecastedBackofficeTasks), analyticsForcastWorkload.ForecastedBackofficeTasks)
			.SetParameter(nameof(analyticsForcastWorkload.ForecastedCampaignCalls), analyticsForcastWorkload.ForecastedCampaignCalls)
			.SetParameter(nameof(analyticsForcastWorkload.ForecastedCallsExclCampaign), analyticsForcastWorkload.ForecastedCallsExclCampaign)
			.SetParameter(nameof(analyticsForcastWorkload.ForecastedTalkTimeSeconds), analyticsForcastWorkload.ForecastedTalkTimeSeconds)
			.SetParameter(nameof(analyticsForcastWorkload.ForecastedCampaignTalkTimeSeconds), analyticsForcastWorkload.ForecastedCampaignTalkTimeSeconds)
			.SetParameter(nameof(analyticsForcastWorkload.ForecastedTalkTimeExclCampaignSeconds), analyticsForcastWorkload.ForecastedTalkTimeExclCampaignSeconds)
			.SetParameter(nameof(analyticsForcastWorkload.ForecastedAfterCallWorkSeconds), analyticsForcastWorkload.ForecastedAfterCallWorkSeconds)
			.SetParameter(nameof(analyticsForcastWorkload.ForecastedCampaignAfterCallWorkSeconds), analyticsForcastWorkload.ForecastedCampaignAfterCallWorkSeconds)
			.SetParameter(nameof(analyticsForcastWorkload.ForecastedAfterCallWorkExclCampaignSeconds), analyticsForcastWorkload.ForecastedAfterCallWorkExclCampaignSeconds)
			.SetParameter(nameof(analyticsForcastWorkload.ForecastedHandlingTimeSeconds), analyticsForcastWorkload.ForecastedHandlingTimeSeconds)
			.SetParameter(nameof(analyticsForcastWorkload.ForecastedCampaignHandlingTimeSeconds), analyticsForcastWorkload.ForecastedCampaignHandlingTimeSeconds)
			.SetParameter(nameof(analyticsForcastWorkload.ForecastedHandlingTimeExclCampaignSeconds), analyticsForcastWorkload.ForecastedHandlingTimeExclCampaignSeconds)
			.SetParameter(nameof(analyticsForcastWorkload.PeriodLengthMinutes), analyticsForcastWorkload.PeriodLengthMinutes)
			.SetParameter(nameof(analyticsForcastWorkload.BusinessUnitId), analyticsForcastWorkload.BusinessUnitId)
			.SetParameter(nameof(analyticsForcastWorkload.DatasourceUpdateDate), analyticsForcastWorkload.DatasourceUpdateDate)
			.ExecuteUpdate();
		}
	}
}