using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Matrix;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsPermissionRepository
	{
		void DeletePermissionsForPerson(Guid personId);
		void InsertPermissions(ICollection<MatrixPermissionHolder> result, Guid businessUnitId);
	}
}