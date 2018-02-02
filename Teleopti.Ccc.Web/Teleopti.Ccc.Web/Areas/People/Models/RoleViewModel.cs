using System;
using System.Diagnostics;

namespace Teleopti.Ccc.Web.Areas.People.Models
{
	[DebuggerDisplay("{Name}, {Id}")]
	public class RoleViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public bool CanBeChangedByCurrentUser { get; set; }
	}
}