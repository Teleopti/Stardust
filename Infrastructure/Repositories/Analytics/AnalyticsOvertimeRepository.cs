using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

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
				$@"exec mart.[etl_dim_overtime_add_or_update]
                    @overtime_code=:{nameof(AnalyticsOvertime.OvertimeCode)}
					,@overtime_name=:{nameof(AnalyticsOvertime.OvertimeName)}
					,@business_unit_id=:{nameof(AnalyticsOvertime.BusinessUnitId)}
					,@datasource_update_date=:{nameof(AnalyticsOvertime.DatasourceUpdateDate)}
					,@is_deleted=:{nameof(AnalyticsOvertime.IsDeleted)}")
				.SetGuid(nameof(AnalyticsOvertime.OvertimeCode), analyticsOvertime.OvertimeCode)
				.SetString(nameof(AnalyticsOvertime.OvertimeName), analyticsOvertime.OvertimeName)
				.SetInt32(nameof(AnalyticsOvertime.BusinessUnitId), analyticsOvertime.BusinessUnitId)
				.SetDateTime(nameof(AnalyticsOvertime.DatasourceUpdateDate), analyticsOvertime.DatasourceUpdateDate)
				.SetBoolean(nameof(AnalyticsOvertime.IsDeleted), analyticsOvertime.IsDeleted);
			query.ExecuteUpdate();
		}

		public IList<AnalyticsOvertime> Overtimes()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select overtime_id {nameof(AnalyticsOvertime.OvertimeId)}, 
						overtime_code {nameof(AnalyticsOvertime.OvertimeCode)},
						overtime_name {nameof(AnalyticsOvertime.OvertimeName)},
						datasource_update_date {nameof(AnalyticsOvertime.DatasourceUpdateDate)},
						is_deleted {nameof(AnalyticsOvertime.IsDeleted)},
						business_unit_id {nameof(AnalyticsOvertime.BusinessUnitId)}
					from mart.dim_overtime WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsOvertime)))
				.SetReadOnly(true)
				.List<AnalyticsOvertime>();
		}
	}
}