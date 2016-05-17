using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics;

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
				@"select 
					 [activity_id] ActivityId
					,[activity_code] ActivityCode
					,[activity_name] ActivityName
					,[display_color] DisplayColor
					,[in_ready_time] InReadyTime
					,[in_ready_time_name] InReadyTimeName
					,[in_contract_time] InContractTime
					,[in_contract_time_name] InContractTimeName
					,[in_paid_time] InPaidTime
					,[in_paid_time_name] InPaidTimeName
					,[in_work_time] InWorkTime
					,[in_work_time_name] InWorkTimeName
					,[business_unit_id] BusinessUnitId
					,[datasource_id] DatasourceId
					,[datasource_update_date] DatasourceUpdateDate
					,[is_deleted] IsDeleted
					,[display_color_html] DisplayColorHtml
					from mart.dim_activity WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsActivity)))
				.SetReadOnly(true)
				.List<AnalyticsActivity>();
		}

		public void AddActivity(AnalyticsActivity activity)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"exec mart.[etl_dim_activity_insert]
						@activity_code=:ActivityCode
						,@activity_name=:ActivityName
						,@display_color=:DisplayColor
						,@in_ready_time=:InReadyTime
						,@in_ready_time_name=:InReadyTimeName
						,@in_contract_time=:InContractTime
						,@in_contract_time_name=:InContractTimeName
						,@in_paid_time=:InPaidTime
						,@in_paid_time_name=:InPaidTimeName
						,@in_work_time=:InWorkTime
						,@in_work_time_name=:InWorkTimeName
						,@business_unit_id=:BusinessUnitId
						,@datasource_id=:DatasourceId
						,@datasource_update_date=:DatasourceUpdateDate
						,@is_deleted=:IsDeleted
						,@display_color_html=:DisplayColorHtml")
				.SetGuid("ActivityCode", activity.ActivityCode)
				.SetString("ActivityName", activity.ActivityName)
				.SetInt32("DisplayColor", activity.DisplayColor)
				.SetBoolean("InReadyTime", activity.InReadyTime)
				.SetString("InReadyTimeName", mapInReadyTimeText(activity.InReadyTime))
				.SetBoolean("InContractTime", activity.InContractTime)
				.SetString("InContractTimeName", mapInContractTimeText(activity.InContractTime))
				.SetBoolean("InPaidTime", activity.InPaidTime)
				.SetString("InPaidTimeName", mapInPaidTimeText(activity.InPaidTime))
				.SetBoolean("InWorkTime", activity.InWorkTime)
				.SetString("InWorkTimeName", mapInWorkTimeText(activity.InWorkTime))
				.SetInt32("BusinessUnitId", activity.BusinessUnitId)
				.SetInt32("DatasourceId", activity.DatasourceId)
				.SetDateTime("DatasourceUpdateDate", activity.DatasourceUpdateDate)
				.SetBoolean("IsDeleted", activity.IsDeleted)
				.SetString("DisplayColorHtml", activity.DisplayColorHtml);
			query.ExecuteUpdate();
		}

		public void UpdateActivity(AnalyticsActivity activity)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				   @"exec mart.[etl_dim_activity_update]
						@activity_code=:ActivityCode
						,@activity_name=:ActivityName
						,@display_color=:DisplayColor
						,@in_ready_time=:InReadyTime
						,@in_ready_time_name=:InReadyTimeName
						,@in_contract_time=:InContractTime
						,@in_contract_time_name=:InContractTimeName
						,@in_paid_time=:InPaidTime
						,@in_paid_time_name=:InPaidTimeName
						,@in_work_time=:InWorkTime
						,@in_work_time_name=:InWorkTimeName
						,@business_unit_id=:BusinessUnitId
						,@datasource_id=:DatasourceId
						,@datasource_update_date=:DatasourceUpdateDate
						,@is_deleted=:IsDeleted
						,@display_color_html=:DisplayColorHtml")
				   .SetGuid("ActivityCode", activity.ActivityCode)
				   .SetString("ActivityName", activity.ActivityName)
				   .SetInt32("DisplayColor", activity.DisplayColor)
				   .SetBoolean("InReadyTime", activity.InReadyTime)
				   .SetString("InReadyTimeName", mapInReadyTimeText(activity.InReadyTime))
				   .SetBoolean("InContractTime", activity.InContractTime)
				   .SetString("InContractTimeName", mapInContractTimeText(activity.InContractTime))
				   .SetBoolean("InPaidTime", activity.InPaidTime)
				   .SetString("InPaidTimeName", mapInPaidTimeText(activity.InPaidTime))
				   .SetBoolean("InWorkTime", activity.InWorkTime)
				   .SetString("InWorkTimeName", mapInWorkTimeText(activity.InWorkTime))
				   .SetInt32("BusinessUnitId", activity.BusinessUnitId)
				   .SetInt32("DatasourceId", activity.DatasourceId)
				   .SetDateTime("DatasourceUpdateDate", activity.DatasourceUpdateDate)
				   .SetBoolean("IsDeleted", activity.IsDeleted)
				   .SetString("DisplayColorHtml", activity.DisplayColorHtml);
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