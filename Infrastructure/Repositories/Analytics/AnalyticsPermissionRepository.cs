using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

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
			runWithBulk(session =>
			{
				foreach (var permission in permissions)
				{
					session.Delete(permission);
				}
			});
		}

		public void InsertPermissions(IEnumerable<AnalyticsPermission> permissions)
		{
			runWithBulk(session =>
			{
				foreach (var permission in permissions)
				{
					session.Insert(permission);
				}
			});
		}

		public IList<AnalyticsPermission> GetPermissionsForPerson(Guid personId, int businessUnitId)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateCriteria<AnalyticsPermission>()
				.Add(Restrictions.Eq(nameof(AnalyticsPermission.PersonCode), personId))
				.Add(Restrictions.Eq(nameof(AnalyticsPermission.BusinessUnitId), businessUnitId));

			var result = query.List<AnalyticsPermission>();
			return result;
		}

		private void runWithBulk(Action<IStatelessSession> action)
		{
			using (var session = _analyticsUnitOfWork.Current().Session().SessionFactory.OpenStatelessSession())
			{
				using (var transaction = session.BeginTransaction())
				{
					try
					{
						action(session);
					}
					catch (Exception)
					{
						transaction.Rollback();
						throw;
					}
					
					transaction.Commit();
				}
			}
		}
	}
}