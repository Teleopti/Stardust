using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsPermissionRepository
	{
		void DeletePermissions(IEnumerable<AnalyticsPermission> permissions);
		void InsertPermissions(IEnumerable<AnalyticsPermission> permissions);
		IList<AnalyticsPermission> GetPermissionsForPerson(Guid personId);
	}
}