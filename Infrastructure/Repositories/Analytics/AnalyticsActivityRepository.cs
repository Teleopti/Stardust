using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
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

		public IList<AnalyticsActivity> Activities()
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
					from mart.dim_activity WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsActivity)))
				.SetReadOnly(true)
				.List<AnalyticsActivity>();
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