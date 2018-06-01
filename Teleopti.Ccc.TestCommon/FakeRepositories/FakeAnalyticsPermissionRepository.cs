using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsPermissionRepository : IAnalyticsPermissionRepository
	{
		private readonly List<AnalyticsPermission> Permissions = new List<AnalyticsPermission>();

		public void DeletePermissions(IEnumerable<AnalyticsPermission> permissions)
		{
			permissions.ForEach(p => Permissions.Remove(p));
		}

		public void InsertPermissions(IEnumerable<AnalyticsPermission> permissions)
		{
			Permissions.AddRange(permissions);
		}

		public IList<AnalyticsPermission> GetPermissionsForPerson(Guid personId, int businessUnitId)
		{
			return Permissions.Where(x => x.PersonCode == personId && x.BusinessUnitId == businessUnitId).ToList();
		}
	}
}