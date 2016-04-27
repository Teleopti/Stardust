using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsPermissionRepository : IAnalyticsPermissionRepository
	{
		private readonly ICurrentDataSource _currentDataSource;

		public AnalyticsPermissionRepository(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public void DeletePermissions(IEnumerable<AnalyticsPermission> permissions)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				using (var transaction = uow.Session().BeginTransaction())
				{
					foreach (var permission in permissions)
					{
						uow.Session().Delete(permission);
					}
					transaction.Commit();
				}
			}
		}

		public void InsertPermissions(IEnumerable<AnalyticsPermission> permissions)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				using (var transaction = uow.Session().BeginTransaction())
				{
					foreach (var permission in permissions)
					{
						uow.Session().Insert(permission);
					}
					transaction.Commit();
				}
			}
		}

		public IList<AnalyticsPermission> GetPermissionsForPerson(Guid personId)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				var query = uow.Session().CreateCriteria<AnalyticsPermission>()
					.Add(Restrictions.Eq("PersonCode", personId));

				var result = query.List<AnalyticsPermission>();
				return result;
			}
		}
	}
}