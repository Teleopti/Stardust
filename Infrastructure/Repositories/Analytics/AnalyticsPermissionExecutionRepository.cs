using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Interfaces.Domain;

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

		public DateTime Get(Guid personId)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(@"
				select update_date 
				from mart.permission_report_execution WITH (NOLOCK)
				where person_code=:PersonCode
				")
				.SetGuid("PersonCode", personId)
				.UniqueResult<DateTime>();
		}

		public void Set(Guid personId)
		{
			var existing = Get(personId);
			string query = @"
					update mart.permission_report_execution
					set update_date=:Now
					where person_code=:PersonCode
					";
			if (existing == DateTime.MinValue)
			{
				query = @"
					insert into mart.permission_report_execution
					values (:PersonCode, :Now)
					";
			}
			_analyticsUnitOfWork.Current().Session().CreateSQLQuery(query)
				.SetGuid("PersonCode", personId)
				.SetDateTime("Now", _now.UtcDateTime())
				.ExecuteUpdate();
		}
	}
}