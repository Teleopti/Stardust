using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Teleopti.Ccc.Web.Areas.People.Models
{

	[DebuggerDisplay("{FirstName} {LastName}, ({Roles.Count()}) {Id}")]
	public class PersonViewModel
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }

		public IEnumerable<RoleViewModel> Roles { get; set; }
	}
}