using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating fake test data for Site object
    /// </summary>
    public static class SiteFactory
    {
        /// <summary>
        /// Creates a unit with no team.
        /// </summary>
        /// <returns></returns>
        public static Site CreateSimpleSite()
        {
            return CreateSimpleSite("New Site");
        }

        /// <summary>
        /// Creates a unit with no team.
        /// </summary>
        /// <returns></returns>
        public static Site CreateSimpleSite(string siteName)
        {
            Site simpleSite = new Site(siteName);
            return simpleSite;
        }


        /// <summary>
        /// Creates site with teams.
        /// </summary>
        /// <param name="teams">The teams.</param>
        /// <returns></returns>
        public static Site CreateSiteWithTeams(IEnumerable<ITeam> teams)
        {
            Site newSite = CreateSimpleSite();
            foreach (ITeam team in teams)
            {
                newSite.AddTeam(team); 
            }
            return newSite;
        }

		public static ISite CreateSiteWithTeam(Guid siteId, string siteName, ITeam team)
		{
			var newSite = CreateSiteWithId(siteId, siteName);
			newSite.AddTeam(team);
			return newSite;
		}

	    /// <summary>
		/// Creates a unit with one team.
		/// </summary>
		/// <returns></returns>
		public static Site CreateSiteWithOneTeam()
        {
            return CreateSiteWithTeams(new List<ITeam> {TeamFactory.CreateSimpleTeam()});
        }

        /// <summary>
        /// Creates a unit with one team.
        /// </summary>
        /// <returns></returns>
        public static Site CreateSiteWithOneTeam(string teamName)
        {
            Site newSite = CreateSimpleSite();
            newSite.AddTeam(TeamFactory.CreateSimpleTeam(teamName));
            return newSite;
        }

	    public static Site CreateSiteWithId(Guid siteId, string sitename)
	    {
		    return CreateSimpleSite(sitename).WithId(siteId);
	    }
    }
}