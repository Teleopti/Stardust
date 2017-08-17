using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Global.Models
{
	public class GroupPagesViewModel
	{
		public SiteViewModelWithTeams[] BusinessHierarchy { get; set; }
		public GroupPageViewModel[] GroupPages { get; set; }
		public Guid? LogonUserTeamId { get; set; }
	}

	public class GroupPageViewModel
	{
		public Guid Id { get; set; }
		public String Name { get; set; }

		public IList<GroupViewModel> Children { get; set; }
	}

	public class GroupViewModel
	{
		public Guid Id { get; set; }
		public String Name { get; set; }
	}
}