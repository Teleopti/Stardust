using System;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for Time related domain objects
    /// </summary>
    public static class TeamFactory
    {
        /// <summary>
        /// Creates a team with no agent.
        /// </summary>
        /// <returns></returns>
        public static Team CreateSimpleTeam()
        {
            return new Team();
        }

        /// <summary>
        /// Creates a team with no agent.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static Team CreateSimpleTeam(string name)
        {
            return new Team().WithDescription(new Description(name));
        }

        /// <summary>
        /// Creates the team with site.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <param name="siteName">Name of the site.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-15
        /// </remarks>
        public static  Team CreateTeam(string teamName,string siteName)
        {
            Site site = SiteFactory.CreateSimpleSite(siteName);
            Team team = new Team().WithDescription(new Description(teamName));
            site.AddTeam(team);

            return team;
        }

	    public static ITeam CreateTeamWithId(string teamName)
	    {
		    var team = CreateSimpleTeam(teamName);
			team.SetId(Guid.NewGuid());
		    return team;
	    }
		
		public static ITeam CreateTeamWithId(Guid teamId, string teamName)
		{
			return CreateSimpleTeam(teamName).WithId(teamId);
		}

		public static ITeam CreateTeamWithId(Guid teamId)
		{
			return CreateSimpleTeam().WithId(teamId);
		}

		public static Team CreateTeamWithSite(string teamName, Site site)
		{
			var team = CreateSimpleTeam(teamName);
			team.Site = site;
			return team;
		}
	}
}