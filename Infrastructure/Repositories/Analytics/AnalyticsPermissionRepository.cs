using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Matrix;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsPermissionRepository : IAnalyticsPermissionRepository
	{
		private readonly ICurrentDataSource _currentDataSource;

		public AnalyticsPermissionRepository(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public void DeletePermissionsForPerson(Guid personId)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				uow.Session().CreateSQLQuery(@"delete from mart.permission_report where person_code=:PersonCode")
					.SetGuid("PersonCode", personId)
					.ExecuteUpdate();
			}
		}

		public void InsertPermissions(ICollection<MatrixPermissionHolder> permissions, Guid businessUnitId)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				foreach (var permission in permissions)
				{
					uow.Session().CreateSQLQuery(@"exec mart.etl_permission_report_insert
						 @person_code=:PersonCode
						,@team_code=:TeamCode
						,@my_own=:MyOwn
						,@business_unit_code=:BusinessUnitCode
						,@report_id=:ReportId")
						.SetGuid("PersonCode", permission.Person.Id.GetValueOrDefault())
						.SetGuid("TeamCode", permission.Team.Id.GetValueOrDefault())
						.SetBoolean("MyOwn", permission.IsMy)
						.SetGuid("BusinessUnitCode", businessUnitId)
						.SetGuid("ReportId", new Guid(permission.ApplicationFunction.ForeignId))
						.ExecuteUpdate();
				}
			}
		}

		public IList<AnalyticsPermission> GetPermissionsForPerson(Guid personId)
		{
			throw new NotImplementedException();
		}
	}
}