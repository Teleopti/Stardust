using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public class AuthorizeMySite : IAuthorizeAvailableData
    {
        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPerson person)
        {
            var targetPeriod = person.Period(dateOnly);
            
            if (targetPeriod == null || targetPeriod.Team == null || targetPeriod.Team.Site == null)
            {
                return false;
            }

            return queryingPerson.BelongsToSite(targetPeriod.Team.Site,dateOnly);
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeam team)
        {
            if (team == null || team.Site == null)
            {
                return false;
            }

            return queryingPerson.BelongsToSite(team.Site,dateOnly);
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISite site)
        {
            if (site == null)
            {
                return false;
            }

            return queryingPerson.BelongsToSite(site,dateOnly);
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IBusinessUnit businessUnit)
        {
            return false;
        }

    	public bool Check(IOrganisationMembershipWithId queryingPerson, DateOnly dateOnly, IAuthorizeOrganisationDetail authorizeOrganisationDetail)
    	{
    		return queryingPerson.BelongsToSite(authorizeOrganisationDetail.SiteId.GetValueOrDefault(), dateOnly);
    	}
    }
}