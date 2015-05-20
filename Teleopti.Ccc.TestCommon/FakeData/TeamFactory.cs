using System;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

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
            Team team = new Team();
            team.Description = new Description(name);
            return team;
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
            Team team = new Team();
            team.Description = new Description(teamName);
            site.AddTeam(team);

            return team;
        }

	    public static ITeam CreateTeamWithId(string teamName)
	    {
		    var team = CreateSimpleTeam(teamName);
			team.SetId(Guid.NewGuid());
		    return team;
	    }
    }
}