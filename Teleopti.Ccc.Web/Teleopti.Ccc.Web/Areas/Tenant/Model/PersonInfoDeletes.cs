using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Tenant.Model
{
	public class PersonInfoDeletes
	{
		public IEnumerable<Guid> PersonIdsToDelete { get; set; }
	}
}