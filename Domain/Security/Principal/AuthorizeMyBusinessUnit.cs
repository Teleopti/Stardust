using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public class AuthorizeMyBusinessUnit : IAuthorizeAvailableData
    {
        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPerson person)
        {
            var targetPeriod = person.Period(dateOnly);

            if (targetPeriod == null || targetPeriod.Team==null)
            {
                return false;
            }

            return queryingPerson.BelongsToBusinessUnit(targetPeriod.Team.BusinessUnitExplicit,dateOnly);
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeam team)
        {
            if (team == null || team.BusinessUnitExplicit == null)
            {
                return false;
            }

            return queryingPerson.BelongsToBusinessUnit(team.BusinessUnitExplicit,dateOnly);
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISite site)
        {
            if (site == null || site.GetOrFillWithBusinessUnit_DONTUSE() == null)
            {
                return false;
            }

            return queryingPerson.BelongsToBusinessUnit(site.GetOrFillWithBusinessUnit_DONTUSE(),dateOnly);
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, Guid businessUnitId)
        {
            if (businessUnitId == Guid.Empty)
            {
                return false;
            }

            return queryingPerson.BelongsToBusinessUnit(businessUnitId,dateOnly);
        }

    	public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPersonAuthorization authorization)
    	{
    		return queryingPerson.BelongsToBusinessUnit(authorization.BusinessUnitId, dateOnly.Date);
    	}

	    public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeamAuthorization authorization)
	    {
			return queryingPerson.BelongsToBusinessUnit(authorization.BusinessUnitId, dateOnly.Date);
		}

	    public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISiteAuthorization authorization)
	    {
			return queryingPerson.BelongsToBusinessUnit(authorization.BusinessUnitId, dateOnly.Date);
		}
	}
}