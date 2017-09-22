using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsWorkloadRepository : IAnalyticsWorkloadRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsWorkloadRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public int AddOrUpdate(AnalyticsWorkload analyticsWorkload)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery($@"
				exec mart.etl_dim_workload_add_or_update
					@workload_code=:{nameof(analyticsWorkload.WorkloadCode)}
					,@workload_name=:{nameof(analyticsWorkload.WorkloadName)}
					,@skill_id=:{nameof(analyticsWorkload.SkillId)}
					,@skill_code=:{nameof(analyticsWorkload.SkillCode)}
					,@skill_name=:{nameof(analyticsWorkload.SkillName)}
					,@time_zone_id=:{nameof(analyticsWorkload.TimeZoneId)}
					,@forecast_method_code=:{nameof(analyticsWorkload.ForecastMethodCode)}
					,@forecast_method_name=:{nameof(analyticsWorkload.ForecastMethodName)}
					,@percentage_offered=:{nameof(analyticsWorkload.PercentageOffered)}
					,@percentage_overflow_in=:{nameof(analyticsWorkload.PercentageOverflowIn)}
					,@percentage_overflow_out=:{nameof(analyticsWorkload.PercentageOverflowOut)}
					,@percentage_abandoned=:{nameof(analyticsWorkload.PercentageAbandoned)}
					,@percentage_abandoned_short=:{nameof(analyticsWorkload.PercentageAbandonedShort)}
					,@percentage_abandoned_within_service_level=:{nameof(analyticsWorkload.PercentageAbandonedWithinServiceLevel)}
					,@percentage_abandoned_after_service_level=:{nameof(analyticsWorkload.PercentageAbandonedAfterServiceLevel)}
					,@business_unit_id=:{nameof(analyticsWorkload.BusinessUnitId)}
					,@datasource_update_date=:{nameof(analyticsWorkload.DatasourceUpdateDate)}
					,@is_deleted=:{nameof(analyticsWorkload.IsDeleted)}
			")
			.SetParameter(nameof(analyticsWorkload.WorkloadCode), analyticsWorkload.WorkloadCode)
			.SetParameter(nameof(analyticsWorkload.WorkloadName), analyticsWorkload.WorkloadName)
			.SetParameter(nameof(analyticsWorkload.SkillId), analyticsWorkload.SkillId)
			.SetParameter(nameof(analyticsWorkload.SkillCode), analyticsWorkload.SkillCode)
			.SetParameter(nameof(analyticsWorkload.SkillName), analyticsWorkload.SkillName)
			.SetParameter(nameof(analyticsWorkload.TimeZoneId), analyticsWorkload.TimeZoneId)
			.SetParameter(nameof(analyticsWorkload.ForecastMethodCode), analyticsWorkload.ForecastMethodCode)
			.SetParameter(nameof(analyticsWorkload.ForecastMethodName), analyticsWorkload.ForecastMethodName)
			.SetParameter(nameof(analyticsWorkload.PercentageOffered), analyticsWorkload.PercentageOffered)
			.SetParameter(nameof(analyticsWorkload.PercentageOverflowIn), analyticsWorkload.PercentageOverflowIn)
			.SetParameter(nameof(analyticsWorkload.PercentageOverflowOut), analyticsWorkload.PercentageOverflowOut)
			.SetParameter(nameof(analyticsWorkload.PercentageAbandoned), analyticsWorkload.PercentageAbandoned)
			.SetParameter(nameof(analyticsWorkload.PercentageAbandonedShort), analyticsWorkload.PercentageAbandonedShort)
			.SetParameter(nameof(analyticsWorkload.PercentageAbandonedWithinServiceLevel), analyticsWorkload.PercentageAbandonedWithinServiceLevel)
			.SetParameter(nameof(analyticsWorkload.PercentageAbandonedAfterServiceLevel), analyticsWorkload.PercentageAbandonedAfterServiceLevel)
			.SetParameter(nameof(analyticsWorkload.BusinessUnitId), analyticsWorkload.BusinessUnitId)
			.SetParameter(nameof(analyticsWorkload.DatasourceUpdateDate), analyticsWorkload.DatasourceUpdateDate)
			.SetParameter(nameof(analyticsWorkload.IsDeleted), analyticsWorkload.IsDeleted)
			.UniqueResult<int>();
		}

		public void AddOrUpdateBridge(AnalyticsBridgeQueueWorkload bridgeQueueWorkload)
		{
			_analyticsUnitOfWork.Current().Session().CreateSQLQuery($@"
				exec mart.etl_bridge_queue_workload_add_or_update
					@queue_id=:{nameof(bridgeQueueWorkload.QueueId)}
					,@workload_id=:{nameof(bridgeQueueWorkload.WorkloadId)}
					,@skill_id=:{nameof(bridgeQueueWorkload.SkillId)}
					,@business_unit_id=:{nameof(bridgeQueueWorkload.BusinessUnitId)}
					,@datasource_update_date=:{nameof(bridgeQueueWorkload.DatasourceUpdateDate)}
			")
			.SetParameter(nameof(bridgeQueueWorkload.QueueId), bridgeQueueWorkload.QueueId)
			.SetParameter(nameof(bridgeQueueWorkload.WorkloadId), bridgeQueueWorkload.WorkloadId)
			.SetParameter(nameof(bridgeQueueWorkload.SkillId), bridgeQueueWorkload.SkillId)
			.SetParameter(nameof(bridgeQueueWorkload.BusinessUnitId), bridgeQueueWorkload.BusinessUnitId)
			.SetParameter(nameof(bridgeQueueWorkload.DatasourceUpdateDate), bridgeQueueWorkload.DatasourceUpdateDate)
			.ExecuteUpdate();
		}

		public IList<AnalyticsBridgeQueueWorkload> GetBridgeQueueWorkloads(int workloadId)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery($@"
					SELECT [queue_id] {nameof(AnalyticsBridgeQueueWorkload.QueueId)}
						  ,[workload_id] {nameof(AnalyticsBridgeQueueWorkload.WorkloadId)}
						  ,[skill_id] {nameof(AnalyticsBridgeQueueWorkload.SkillId)}
						  ,[business_unit_id] {nameof(AnalyticsBridgeQueueWorkload.BusinessUnitId)}
						  ,[datasource_id] {nameof(AnalyticsBridgeQueueWorkload.DatasourceId)}
						  ,[insert_date] {nameof(AnalyticsBridgeQueueWorkload.InsertDate)}
						  ,[update_date] {nameof(AnalyticsBridgeQueueWorkload.UpdateDate)}
						  ,[datasource_update_date] {nameof(AnalyticsBridgeQueueWorkload.DatasourceUpdateDate)}
					FROM [mart].[bridge_queue_workload] WITH (NOLOCK)
					WHERE [workload_id]=:{nameof(workloadId)}	
				")
				.SetParameter(nameof(workloadId), workloadId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsBridgeQueueWorkload)))
				.List<AnalyticsBridgeQueueWorkload>();
		}

		public void DeleteBridge(int workloadId, int queueId)
		{
			_analyticsUnitOfWork.Current().Session().CreateSQLQuery($@"
					exec mart.etl_bridge_queue_workload_delete
						@workload_id=:{nameof(workloadId)}
						,@queue_id=:{nameof(queueId)}
				")
				.SetParameter(nameof(workloadId), workloadId)
				.SetParameter(nameof(queueId), queueId)
				.ExecuteUpdate();
		}

		public AnalyticsWorkload GetWorkload(Guid workloadCode)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery($@"
					SELECT 
						[workload_id] {nameof(AnalyticsWorkload.WorkloadId)}
						,[workload_code] {nameof(AnalyticsWorkload.WorkloadCode)}
						,[workload_name] {nameof(AnalyticsWorkload.WorkloadName)}
						,[skill_id] {nameof(AnalyticsWorkload.SkillId)}
						,[skill_code] {nameof(AnalyticsWorkload.SkillCode)}
						,[skill_name] {nameof(AnalyticsWorkload.SkillName)}
						,[time_zone_id] {nameof(AnalyticsWorkload.TimeZoneId)}
						,[forecast_method_code] {nameof(AnalyticsWorkload.ForecastMethodCode)}
						,[forecast_method_name] {nameof(AnalyticsWorkload.ForecastMethodName)}
						,[percentage_offered] {nameof(AnalyticsWorkload.PercentageOffered)}
						,[percentage_overflow_in] {nameof(AnalyticsWorkload.PercentageOverflowIn)}
						,[percentage_overflow_out] {nameof(AnalyticsWorkload.PercentageOverflowOut)}
						,[percentage_abandoned] {nameof(AnalyticsWorkload.PercentageAbandoned)}
						,[percentage_abandoned_short] {nameof(AnalyticsWorkload.PercentageAbandonedShort)}
						,[percentage_abandoned_within_service_level] {nameof(AnalyticsWorkload.PercentageAbandonedWithinServiceLevel)}
						,[percentage_abandoned_after_service_level] {nameof(AnalyticsWorkload.PercentageAbandonedAfterServiceLevel)}
						,[business_unit_id] {nameof(AnalyticsWorkload.BusinessUnitId)}
						,[datasource_id] {nameof(AnalyticsWorkload.DatasourceId)}
						,[insert_date] {nameof(AnalyticsWorkload.InsertDate)}
						,[update_date] {nameof(AnalyticsWorkload.UpdateDate)}
						,[datasource_update_date] {nameof(AnalyticsWorkload.DatasourceUpdateDate)}
						,[is_deleted] {nameof(AnalyticsWorkload.IsDeleted)}
					FROM [mart].[dim_workload] WITH (NOLOCK)
					WHERE [workload_code]=:{nameof(workloadCode)}	
				")
			.SetParameter(nameof(workloadCode), workloadCode)
			.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsWorkload)))
			.UniqueResult<AnalyticsWorkload>();
		}
	}
}