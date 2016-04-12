using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Matrix;

namespace Teleopti.Ccc.Web.Areas.Reporting.Core
{
	public interface IApplicationPermissionProvider
	{
		ICollection<MatrixPermissionHolder> GetPermissions(Guid personId);
	}
}