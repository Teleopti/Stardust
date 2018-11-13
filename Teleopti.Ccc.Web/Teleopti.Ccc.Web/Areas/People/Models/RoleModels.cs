﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Teleopti.Ccc.Web.Areas.People.Models
{
	/// <summary>
	/// ViewModels
	/// </summary>
	[DebuggerDisplay("{Name}, {Id}")]
	public class RoleViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public bool CanBeChangedByCurrentUser { get; set; }
	}

	[DebuggerDisplay("{FirstName} {LastName}, ({Roles.Count()}) {Id}")]
	public class PersonViewModel
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }

		public IEnumerable<RoleViewModel> Roles { get; set; }
	}

	/// <summary>
	/// Inputmodels
	/// </summary>
	public class FecthPersonsInputModel
	{
		public DateTime Date { get; set; }
		public IEnumerable<Guid> PersonIdList { get; set; }
	}

	public class PersonRoleInputModel
	{
		public Guid PersonId { get; set; }
		public IEnumerable<Guid> Roles { get; set; }
	}

	
}