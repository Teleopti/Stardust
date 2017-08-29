using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class Organization
	{
		public IBusinessUnit BusinessUnit { get; set; }
		public ISite Site { get; set; }
		public ITeam Team { get; set; }
	}

	public static class OrganizationFactory
	{
		public static Organization Create(Guid businessUnitId, Guid siteId, string sitename, Guid teamId, string teamname)
		{
			var businessUnit = BusinessUnitFactory.CreateWithId(businessUnitId);
			var team = TeamFactory.CreateTeamWithId(teamId, "teamName");
			var site = SiteFactory.CreateSiteWithTeam(siteId, "siteName", team);
			site.SetBusinessUnit(businessUnit);
			site.AddTeam(team);
			return new Organization
			{
				BusinessUnit = businessUnit,
				Site = site,
				Team = team
			};
		}


	}
}