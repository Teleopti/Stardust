using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsOvertimeRepository : IAnalyticsOvertimeRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsOvertimeRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public void AddOrUpdate(AnalyticsOvertime analyticsOvertime)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"exec mart.[etl_dim_overtime_add_or_update]
                    @overtime_code=:overtime_code
					,@overtime_name=:overtime_name
					,@business_unit_id=:business_unit_id
					,@datasource_update_date=:datasource_update_date
					,@is_deleted=:is_deleted")
				.SetGuid("overtime_code", analyticsOvertime.OvertimeCode)
				.SetString("overtime_name", analyticsOvertime.OvertimeName)
				.SetInt32("business_unit_id", analyticsOvertime.BusinessUnitId)
				.SetDateTime("datasource_update_date", analyticsOvertime.DatasourceUpdateDate)
				.SetBoolean("is_deleted", analyticsOvertime.IsDeleted);
			query.ExecuteUpdate();
		}
	}
}