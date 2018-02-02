using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.People.Models
{
	public class PersonRoleInputModel
	{
		public Guid PersonId { get; set; }
		public IEnumerable<Guid> Roles { get; set; }
	}
}