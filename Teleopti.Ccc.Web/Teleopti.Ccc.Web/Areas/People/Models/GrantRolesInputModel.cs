using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.People.Models
{
	public class GrantRolesInputModel
	{
		public IEnumerable<Guid> Persons { get; set; }
		public IEnumerable<Guid> Roles { get; set; }
	}
}