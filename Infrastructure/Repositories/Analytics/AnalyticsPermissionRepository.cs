using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsPermissionRepository : IAnalyticsPermissionRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsPermissionRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public void DeletePermissions(IEnumerable<AnalyticsPermission> permissions)
		{
			foreach (var permission in permissions)
			{
				_analyticsUnitOfWork.Current().Session().Delete(permission);
			}
		}

		public void InsertPermissions(IEnumerable<AnalyticsPermission> permissions)
		{
			foreach (var permission in permissions)
			{
				_analyticsUnitOfWork.Current().Session().Save(permission);
			}
		}

		public IList<AnalyticsPermission> GetPermissionsForPerson(Guid personId)
		{

			var query = _analyticsUnitOfWork.Current().Session().CreateCriteria<AnalyticsPermission>()
				.Add(Restrictions.Eq("PersonCode", personId));

			var result = query.List<AnalyticsPermission>();
			return result;
		}
	}
}