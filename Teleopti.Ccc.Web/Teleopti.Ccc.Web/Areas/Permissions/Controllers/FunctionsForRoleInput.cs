using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Permissions.Controllers
{
	public struct FunctionsForRoleInput
	{
		public ICollection<Guid> Functions { get; set; }
	}
}