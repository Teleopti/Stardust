using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsPermissionExecutionRepository : IAnalyticsPermissionExecutionRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;
		private readonly INow _now;

		public AnalyticsPermissionExecutionRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork, INow now)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
			_now = now;
		}

		public DateTime Get(Guid personId, int businessUnitId)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery($@"
				select update_date 
				from mart.permission_report_execution WITH (NOLOCK)
				where person_code=:{nameof(personId)} AND business_unit_id=:{nameof(businessUnitId)}
				")
				.SetParameter(nameof(personId), personId)
				.SetParameter(nameof(businessUnitId), businessUnitId)
				.UniqueResult<DateTime>();
		}

		public void Set(Guid personId, int businessUnitId)
		{
			var existing = Get(personId, businessUnitId);
			var now = _now.UtcDateTime();
			string query = $@"
					update mart.permission_report_execution
					set update_date=:{nameof(now)}
					where person_code=:{nameof(personId)} AND business_unit_id=:{nameof(businessUnitId)}
					";
			if (existing == DateTime.MinValue)
			{
				query = $@"
					insert into mart.permission_report_execution
					values (:{nameof(personId)}, :{nameof(businessUnitId)}, :{nameof(now)})
					";
			}
			_analyticsUnitOfWork.Current().Session().CreateSQLQuery(query)
				.SetParameter(nameof(personId), personId)
				.SetParameter(nameof(now), now)
				.SetParameter(nameof(businessUnitId), businessUnitId)
				.ExecuteUpdate();
		}
	}
}