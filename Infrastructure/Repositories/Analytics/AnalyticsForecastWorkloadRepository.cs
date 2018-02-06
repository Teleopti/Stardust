using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
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

		public void Delete(AnalyticsForcastWorkload workload)
		{
			_analyticsUnitOfWork.Current().Session().CreateSQLQuery($@"
				exec mart.[etl_fact_forecast_workload_delete]
				@date_id=:{nameof(workload.DateId)}
				,@interval_id=:{nameof(workload.IntervalId)}
				,@workload_id=:{nameof(workload.WorkloadId)}
				,@scenario_id=:{nameof(workload.ScenarioId)}
				,@start_time=:{nameof(workload.StartTime)}
			")
			.SetParameter(nameof(workload.DateId), workload.DateId)
			.SetParameter(nameof(workload.IntervalId), workload.IntervalId)
			.SetParameter(nameof(workload.WorkloadId), workload.WorkloadId)
			.SetParameter(nameof(workload.ScenarioId), workload.ScenarioId)
			.SetParameter(nameof(workload.StartTime), workload.StartTime)
			.ExecuteUpdate();
		}

		public IList<AnalyticsForcastWorkload> GetForecastWorkloads(int workloadId, int scenarioId, int startDateId, int endDateId, int startIntervalId, int endIntervalId)
		{
			return _analyticsUnitOfWork.Current()
					.Session()
					.CreateSQLQuery($@"
					SELECT [date_id] {nameof(AnalyticsForcastWorkload.DateId)}
						  ,[interval_id] {nameof(AnalyticsForcastWorkload.IntervalId)}
						  ,[start_time] {nameof(AnalyticsForcastWorkload.StartTime)}
						  ,[workload_id] {nameof(AnalyticsForcastWorkload.WorkloadId)}
						  ,[scenario_id] {nameof(AnalyticsForcastWorkload.ScenarioId)}
						  ,[end_time] {nameof(AnalyticsForcastWorkload.EndTime)}
						  ,[skill_id] {nameof(AnalyticsForcastWorkload.SkillId)}
						  ,[forecasted_calls] {nameof(AnalyticsForcastWorkload.ForecastedCalls)}
						  ,[forecasted_emails] {nameof(AnalyticsForcastWorkload.ForecastedEmails)}
						  ,[forecasted_backoffice_tasks] {nameof(AnalyticsForcastWorkload.ForecastedBackofficeTasks)}
						  ,[forecasted_campaign_calls] {nameof(AnalyticsForcastWorkload.ForecastedCampaignCalls)}
						  ,[forecasted_calls_excl_campaign] {nameof(AnalyticsForcastWorkload.ForecastedCallsExclCampaign)}
						  ,[forecasted_talk_time_s] {nameof(AnalyticsForcastWorkload.ForecastedTalkTimeSeconds)}
						  ,[forecasted_campaign_talk_time_s] {nameof(AnalyticsForcastWorkload.ForecastedCampaignTalkTimeSeconds)}
						  ,[forecasted_talk_time_excl_campaign_s] {nameof(AnalyticsForcastWorkload.ForecastedTalkTimeExclCampaignSeconds)}
						  ,[forecasted_after_call_work_s] {nameof(AnalyticsForcastWorkload.ForecastedAfterCallWorkSeconds)}
						  ,[forecasted_campaign_after_call_work_s] {nameof(AnalyticsForcastWorkload.ForecastedCampaignAfterCallWorkSeconds)}
						  ,[forecasted_after_call_work_excl_campaign_s] {nameof(AnalyticsForcastWorkload.ForecastedAfterCallWorkExclCampaignSeconds)}
						  ,[forecasted_handling_time_s] {nameof(AnalyticsForcastWorkload.ForecastedHandlingTimeSeconds)}
						  ,[forecasted_campaign_handling_time_s] {nameof(AnalyticsForcastWorkload.ForecastedCampaignHandlingTimeSeconds)}
						  ,[forecasted_handling_time_excl_campaign_s] {nameof(AnalyticsForcastWorkload.ForecastedHandlingTimeExclCampaignSeconds)}
						  ,[period_length_min] {nameof(AnalyticsForcastWorkload.PeriodLengthMinutes)}
						  ,[business_unit_id] {nameof(AnalyticsForcastWorkload.BusinessUnitId)}
						  ,[datasource_id] {nameof(AnalyticsForcastWorkload.DatasourceId)}
						  ,[insert_date] {nameof(AnalyticsForcastWorkload.InsertDate)}
						  ,[update_date] {nameof(AnalyticsForcastWorkload.UpdateDate)}
						  ,[datasource_update_date] {nameof(AnalyticsForcastWorkload.DatasourceUpdateDate)}
					FROM [mart].[fact_forecast_workload] 
					WHERE [workload_id]=:{nameof(workloadId)}
					AND [scenario_id]=:{nameof(scenarioId)}
					AND (
							([date_id]=:{nameof(startDateId)} AND [interval_id] >=:{nameof(startIntervalId)}) 
							OR 
							([date_id]=:{nameof(endDateId)} AND [interval_id] <=:{nameof(endIntervalId)})
						)
					")
					.AddScalar(nameof(AnalyticsForcastWorkload.DateId), NHibernateUtil.Int32)
					.AddScalar(nameof(AnalyticsForcastWorkload.IntervalId), NHibernateUtil.Int32)
					.AddScalar(nameof(AnalyticsForcastWorkload.StartTime), NHibernateUtil.DateTime)
					.AddScalar(nameof(AnalyticsForcastWorkload.WorkloadId), NHibernateUtil.Int32)
					.AddScalar(nameof(AnalyticsForcastWorkload.ScenarioId), NHibernateUtil.Int32)
					.AddScalar(nameof(AnalyticsForcastWorkload.EndTime), NHibernateUtil.DateTime)
					.AddScalar(nameof(AnalyticsForcastWorkload.SkillId), NHibernateUtil.Int32)
					.AddScalar(nameof(AnalyticsForcastWorkload.ForecastedCalls), NHibernateUtil.Double)
					.AddScalar(nameof(AnalyticsForcastWorkload.ForecastedEmails), NHibernateUtil.Double)
					.AddScalar(nameof(AnalyticsForcastWorkload.ForecastedBackofficeTasks), NHibernateUtil.Double)
					.AddScalar(nameof(AnalyticsForcastWorkload.ForecastedCampaignCalls), NHibernateUtil.Double)
					.AddScalar(nameof(AnalyticsForcastWorkload.ForecastedCallsExclCampaign), NHibernateUtil.Double)
					.AddScalar(nameof(AnalyticsForcastWorkload.ForecastedTalkTimeSeconds), NHibernateUtil.Double)
					.AddScalar(nameof(AnalyticsForcastWorkload.ForecastedCampaignTalkTimeSeconds), NHibernateUtil.Double)
					.AddScalar(nameof(AnalyticsForcastWorkload.ForecastedTalkTimeExclCampaignSeconds), NHibernateUtil.Double)
					.AddScalar(nameof(AnalyticsForcastWorkload.ForecastedAfterCallWorkSeconds), NHibernateUtil.Double)
					.AddScalar(nameof(AnalyticsForcastWorkload.ForecastedCampaignAfterCallWorkSeconds), NHibernateUtil.Double)
					.AddScalar(nameof(AnalyticsForcastWorkload.ForecastedAfterCallWorkExclCampaignSeconds), NHibernateUtil.Double)
					.AddScalar(nameof(AnalyticsForcastWorkload.ForecastedHandlingTimeSeconds), NHibernateUtil.Double)
					.AddScalar(nameof(AnalyticsForcastWorkload.ForecastedCampaignHandlingTimeSeconds), NHibernateUtil.Double)
					.AddScalar(nameof(AnalyticsForcastWorkload.ForecastedHandlingTimeExclCampaignSeconds), NHibernateUtil.Double)
					.AddScalar(nameof(AnalyticsForcastWorkload.PeriodLengthMinutes), NHibernateUtil.Double)
					.AddScalar(nameof(AnalyticsForcastWorkload.BusinessUnitId), NHibernateUtil.Int32)
					.AddScalar(nameof(AnalyticsForcastWorkload.DatasourceId), NHibernateUtil.Int32)
					.AddScalar(nameof(AnalyticsForcastWorkload.InsertDate), NHibernateUtil.DateTime)
					.AddScalar(nameof(AnalyticsForcastWorkload.UpdateDate), NHibernateUtil.DateTime)
					.AddScalar(nameof(AnalyticsForcastWorkload.DatasourceUpdateDate), NHibernateUtil.DateTime)
					.SetParameter(nameof(workloadId), workloadId)
					.SetParameter(nameof(scenarioId), scenarioId)
					.SetParameter(nameof(startDateId), startDateId)
					.SetParameter(nameof(endDateId), endDateId)
					.SetParameter(nameof(startIntervalId), startIntervalId)
					.SetParameter(nameof(endIntervalId), endIntervalId)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsForcastWorkload)))
					.SetReadOnly(true)
					.List<AnalyticsForcastWorkload>();
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