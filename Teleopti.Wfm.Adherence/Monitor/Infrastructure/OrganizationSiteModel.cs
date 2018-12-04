using System;
using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.Monitor.Infrastructure
{
	public class OrganizationSiteModel
	{
		public Guid BusinessUnitId { get; set; }
		public Guid SiteId { get; set; }
		public string SiteName { get; set; }
		public IEnumerable<OrganizationTeamModel> Teams;
	}

	public class OrganizationTeamModel
	{
		public Guid TeamId { get; set; }
		public string TeamName { get; set; }
	}

}