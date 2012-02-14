using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public class AuthorizeMyTeam : IAuthorizeAvailableData
    {
        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPerson person)
        {
            var targetPeriod = person.Period(dateOnly);
            
            if (targetPeriod==null || targetPeriod.Team ==null)
            {
                return false;
            }

            return queryingPerson.BelongsToTeam(targetPeriod.Team,dateOnly);
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeam team)
        {
            if (team == null)
            {
                return false;
            }

            return queryingPerson.BelongsToTeam(team, dateOnly);
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISite site)
        {
            return false;
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IBusinessUnit businessUnit)
        {
            return false;
        }

    	public bool Check(IOrganisationMembershipWithId queryingPerson, DateOnly dateOnly, IAuthorizeOrganisationDetail authorizeOrganisationDetail)
    	{
    		return queryingPerson.BelongsToTeam(authorizeOrganisationDetail.TeamId.GetValueOrDefault(), dateOnly);
    	}
    }
}