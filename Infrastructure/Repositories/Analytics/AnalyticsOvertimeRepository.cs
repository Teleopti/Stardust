using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

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

		public IList<AnalyticsOvertime> Overtimes()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"select overtime_id OvertimeId, 
						overtime_code OvertimeCode,
						overtime_name OvertimeName,
						datasource_update_date DatasourceUpdateDate,
						is_deleted IsDeleted,
						business_unit_id BusinessUnitId
					from mart.dim_overtime WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsOvertime)))
				.SetReadOnly(true)
				.List<AnalyticsOvertime>();
		}
	}
}