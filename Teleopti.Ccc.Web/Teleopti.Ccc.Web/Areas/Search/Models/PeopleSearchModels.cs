using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Teleopti.Ccc.Web.Areas.Search.Models
{
	public class FindPeopleInputModel
	{
		public DateTime SearchDate { get; set; }
		public string KeyWord { get; set; }
		public int PageSize { get; set; } 
		public int PageIndex { get; set; }
		public int Direction { get; set; }
		public int SortColumn { get; set; }
	}

	public class FindPeopleViewModel
	{
		public FindPeopleViewModel()
		{
			People = new List<PeopleViewModel>();
		}

		public int TotalRows { get; set; }
		public int PageIndex { get; set; }
		public List<PeopleViewModel> People { get; set; }
	}

	[DebuggerDisplay("{FirstName} {LastName}, {Site}, {Team}")]
	public class PeopleViewModel
	{
		public string Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string SiteTeam { get; set; }
		public string Team { get; set; }
		public string Site { get; set; }
		public List<SearchPersonRoleViewModel> Roles { get; set; }
	}
	public class SearchPersonRoleViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}