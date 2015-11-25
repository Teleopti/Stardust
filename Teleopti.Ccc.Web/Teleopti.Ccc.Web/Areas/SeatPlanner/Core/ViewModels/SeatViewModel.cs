using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels
{
	public class SeatViewModel
	{
		public Guid Id { get; set; }
		public String Name { get; set; }
		public bool IsOccupied { get; set; }
		public List<Guid> RoleIdList{ get; set; }
		public int Priority	{ get; set; }
	}
}
