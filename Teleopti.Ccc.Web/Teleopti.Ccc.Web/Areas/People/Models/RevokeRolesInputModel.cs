using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.ModelBinding;

namespace Teleopti.Ccc.Web.Areas.People.Models
{
	public class RevokeRolesInputModel
	{
		public IEnumerable<Guid> Persons { get; set; }
		public IEnumerable<Guid> Roles { get; set; }
	}
}