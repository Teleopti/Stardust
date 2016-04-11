using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsPermissionExecutionRepository : IAnalyticsPermissionExecutionRepository
	{
		private readonly ICurrentDataSource _currentDataSource;
		private readonly INow _now;

		public AnalyticsPermissionExecutionRepository(ICurrentDataSource currentDataSource, INow now)
		{
			_currentDataSource = currentDataSource;
			_now = now;
		}

		public DateTime Get(Guid personId)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(@"
					select update_date 
					from mart.permission_report_execution WITH (NOLOCK)
					where person_code=:PersonCode
					")
					.SetGuid("PersonCode", personId)
					.UniqueResult<DateTime>();
			}
		}

		public void Set(Guid personId)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
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
				uow.Session().CreateSQLQuery(query)
					.SetGuid("PersonCode", personId)
					.SetDateTime("Now", _now.UtcDateTime())
					.ExecuteUpdate();
			}
		}
	}
}