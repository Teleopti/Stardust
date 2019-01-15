using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsActivityRepository : IAnalyticsActivityRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsActivityRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}
		
		public void AddActivity(AnalyticsActivity activity)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_dim_activity_insert]
						@activity_code=:{nameof(AnalyticsActivity.ActivityCode)}
						,@activity_name=:{nameof(AnalyticsActivity.ActivityName)}
						,@display_color=:{nameof(AnalyticsActivity.DisplayColor)}
						,@in_ready_time=:{nameof(AnalyticsActivity.InReadyTime)}
						,@in_ready_time_name=:{nameof(AnalyticsActivity.InReadyTimeName)}
						,@in_contract_time=:{nameof(AnalyticsActivity.InContractTime)}
						,@in_contract_time_name=:{nameof(AnalyticsActivity.InContractTimeName)}
						,@in_paid_time=:{nameof(AnalyticsActivity.InPaidTime)}
						,@in_paid_time_name=:{nameof(AnalyticsActivity.InPaidTimeName)}
						,@in_work_time=:{nameof(AnalyticsActivity.InWorkTime)}
						,@in_work_time_name=:{nameof(AnalyticsActivity.InWorkTimeName)}
						,@business_unit_id=:{nameof(AnalyticsActivity.BusinessUnitId)}
						,@datasource_id=:{nameof(AnalyticsActivity.DatasourceId)}
						,@datasource_update_date=:{nameof(AnalyticsActivity.DatasourceUpdateDate)}
						,@is_deleted=:{nameof(AnalyticsActivity.IsDeleted)}
						,@display_color_html=:{nameof(AnalyticsActivity.DisplayColorHtml)}")
				.SetGuid(nameof(AnalyticsActivity.ActivityCode), activity.ActivityCode)
				.SetString(nameof(AnalyticsActivity.ActivityName), activity.ActivityName)
				.SetInt32(nameof(AnalyticsActivity.DisplayColor), activity.DisplayColor)
				.SetBoolean(nameof(AnalyticsActivity.InReadyTime), activity.InReadyTime)
				.SetString(nameof(AnalyticsActivity.InReadyTimeName), mapInReadyTimeText(activity.InReadyTime))
				.SetBoolean(nameof(AnalyticsActivity.InContractTime), activity.InContractTime)
				.SetString(nameof(AnalyticsActivity.InContractTimeName), mapInContractTimeText(activity.InContractTime))
				.SetBoolean(nameof(AnalyticsActivity.InPaidTime), activity.InPaidTime)
				.SetString(nameof(AnalyticsActivity.InPaidTimeName), mapInPaidTimeText(activity.InPaidTime))
				.SetBoolean(nameof(AnalyticsActivity.InWorkTime), activity.InWorkTime)
				.SetString(nameof(AnalyticsActivity.InWorkTimeName), mapInWorkTimeText(activity.InWorkTime))
				.SetInt32(nameof(AnalyticsActivity.BusinessUnitId), activity.BusinessUnitId)
				.SetInt32(nameof(AnalyticsActivity.DatasourceId), activity.DatasourceId)
				.SetDateTime(nameof(AnalyticsActivity.DatasourceUpdateDate), activity.DatasourceUpdateDate)
				.SetBoolean(nameof(AnalyticsActivity.IsDeleted), activity.IsDeleted)
				.SetString(nameof(AnalyticsActivity.DisplayColorHtml), activity.DisplayColorHtml);
			query.ExecuteUpdate();
		}

		public void UpdateActivity(AnalyticsActivity activity)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				   $@"exec mart.[etl_dim_activity_update]
						@activity_code=:{nameof(AnalyticsActivity.ActivityCode)}
						,@activity_name=:{nameof(AnalyticsActivity.ActivityName)}
						,@display_color=:{nameof(AnalyticsActivity.DisplayColor)}
						,@in_ready_time=:{nameof(AnalyticsActivity.InReadyTime)}
						,@in_ready_time_name=:{nameof(AnalyticsActivity.InReadyTimeName)}
						,@in_contract_time=:{nameof(AnalyticsActivity.InContractTime)}
						,@in_contract_time_name=:{nameof(AnalyticsActivity.InContractTimeName)}
						,@in_paid_time=:{nameof(AnalyticsActivity.InPaidTime)}
						,@in_paid_time_name=:{nameof(AnalyticsActivity.InPaidTimeName)}
						,@in_work_time=:{nameof(AnalyticsActivity.InWorkTime)}
						,@in_work_time_name=:{nameof(AnalyticsActivity.InWorkTimeName)}
						,@business_unit_id=:{nameof(AnalyticsActivity.BusinessUnitId)}
						,@datasource_id=:{nameof(AnalyticsActivity.DatasourceId)}
						,@datasource_update_date=:{nameof(AnalyticsActivity.DatasourceUpdateDate)}
						,@is_deleted=:{nameof(AnalyticsActivity.IsDeleted)}
						,@display_color_html=:{nameof(AnalyticsActivity.DisplayColorHtml)}")
				   .SetGuid(nameof(AnalyticsActivity.ActivityCode), activity.ActivityCode)
				   .SetString(nameof(AnalyticsActivity.ActivityName), activity.ActivityName)
				   .SetInt32(nameof(AnalyticsActivity.DisplayColor), activity.DisplayColor)
				   .SetBoolean(nameof(AnalyticsActivity.InReadyTime), activity.InReadyTime)
				   .SetString(nameof(AnalyticsActivity.InReadyTimeName), mapInReadyTimeText(activity.InReadyTime))
				   .SetBoolean(nameof(AnalyticsActivity.InContractTime), activity.InContractTime)
				   .SetString(nameof(AnalyticsActivity.InContractTimeName), mapInContractTimeText(activity.InContractTime))
				   .SetBoolean(nameof(AnalyticsActivity.InPaidTime), activity.InPaidTime)
				   .SetString(nameof(AnalyticsActivity.InPaidTimeName), mapInPaidTimeText(activity.InPaidTime))
				   .SetBoolean(nameof(AnalyticsActivity.InWorkTime), activity.InWorkTime)
				   .SetString(nameof(AnalyticsActivity.InWorkTimeName), mapInWorkTimeText(activity.InWorkTime))
				   .SetInt32(nameof(AnalyticsActivity.BusinessUnitId), activity.BusinessUnitId)
				   .SetInt32(nameof(AnalyticsActivity.DatasourceId), activity.DatasourceId)
				   .SetDateTime(nameof(AnalyticsActivity.DatasourceUpdateDate), activity.DatasourceUpdateDate)
				   .SetBoolean(nameof(AnalyticsActivity.IsDeleted), activity.IsDeleted)
				   .SetString(nameof(AnalyticsActivity.DisplayColorHtml), activity.DisplayColorHtml);
			query.ExecuteUpdate();
		}

		public AnalyticsActivity Activity(Guid code)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
					$@"select 
					 [activity_id] {nameof(AnalyticsActivity.ActivityId)}
					,[activity_code] {nameof(AnalyticsActivity.ActivityCode)}
					,[activity_name] {nameof(AnalyticsActivity.ActivityName)}
					,[display_color] {nameof(AnalyticsActivity.DisplayColor)}
					,[in_ready_time] {nameof(AnalyticsActivity.InReadyTime)}
					,[in_ready_time_name] {nameof(AnalyticsActivity.InReadyTimeName)}
					,[in_contract_time] {nameof(AnalyticsActivity.InContractTime)}
					,[in_contract_time_name] {nameof(AnalyticsActivity.InContractTimeName)}
					,[in_paid_time] {nameof(AnalyticsActivity.InPaidTime)}
					,[in_paid_time_name] {nameof(AnalyticsActivity.InPaidTimeName)}
					,[in_work_time] {nameof(AnalyticsActivity.InWorkTime)}
					,[in_work_time_name] {nameof(AnalyticsActivity.InWorkTimeName)}
					,[business_unit_id] {nameof(AnalyticsActivity.BusinessUnitId)}
					,[datasource_id] {nameof(AnalyticsActivity.DatasourceId)}
					,[datasource_update_date] {nameof(AnalyticsActivity.DatasourceUpdateDate)}
					,[is_deleted] {nameof(AnalyticsActivity.IsDeleted)}
					,[display_color_html] {nameof(AnalyticsActivity.DisplayColorHtml)}
					from mart.dim_activity WITH (NOLOCK) WHERE activity_code = :{nameof(AnalyticsActivity.ActivityCode)}")
				.SetGuid(nameof(AnalyticsActivity.ActivityCode),code)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsActivity)))
				.SetReadOnly(true)
				.UniqueResult<AnalyticsActivity>();
		}

		public IList<AnalyticsActivity> Activities()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
					$@"select 
					 a.[activity_id] {nameof(AnalyticsActivity.ActivityId)}
					,a.[activity_code] {nameof(AnalyticsActivity.ActivityCode)}
					,a.[activity_name] {nameof(AnalyticsActivity.ActivityName)}
					,a.[display_color] {nameof(AnalyticsActivity.DisplayColor)}
					,a.[in_ready_time] {nameof(AnalyticsActivity.InReadyTime)}
					,a.[in_ready_time_name] {nameof(AnalyticsActivity.InReadyTimeName)}
					,a.[in_contract_time] {nameof(AnalyticsActivity.InContractTime)}
					,a.[in_contract_time_name] {nameof(AnalyticsActivity.InContractTimeName)}
					,a.[in_paid_time] {nameof(AnalyticsActivity.InPaidTime)}
					,a.[in_paid_time_name] {nameof(AnalyticsActivity.InPaidTimeName)}
					,a.[in_work_time] {nameof(AnalyticsActivity.InWorkTime)}
					,a.[in_work_time_name] {nameof(AnalyticsActivity.InWorkTimeName)}
					,a.[business_unit_id] {nameof(AnalyticsActivity.BusinessUnitId)}
					,a.[datasource_id] {nameof(AnalyticsActivity.DatasourceId)}
					,a.[datasource_update_date] {nameof(AnalyticsActivity.DatasourceUpdateDate)}
					,a.[is_deleted] {nameof(AnalyticsActivity.IsDeleted)}
					,a.[display_color_html] {nameof(AnalyticsActivity.DisplayColorHtml)}
					from mart.dim_activity a WITH (NOLOCK) INNER JOIN mart.dim_business_unit bu WITH (NOLOCK) ON bu.[business_unit_id] = a.[business_unit_id] WHERE bu.[business_unit_code] = :businessUnit")
				.SetGuid("businessUnit",getBusinessUnitId())
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsActivity)))
				.SetReadOnly(true)
				.List<AnalyticsActivity>();
		}

		private Guid getBusinessUnitId()
		{
			return ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.GetValueOrDefault();
		}

		private string mapInReadyTimeText(bool inReadyTime)
		{
			return inReadyTime ? "In Ready Time" : "Not In Ready Time";
		}

		private string mapInContractTimeText(bool inContractTime)
		{
			return inContractTime ? "In Contract Time" : "Not In Contract Time";
		}

		private string mapInPaidTimeText(bool inPaidTime)
		{
			return inPaidTime ? "In Paid Time" : "Not In Paid Time";
		}

		private string mapInWorkTimeText(bool inWorkTime)
		{
			return inWorkTime ? "In Work Time" : "Not In Work Time";
		}
	}
}