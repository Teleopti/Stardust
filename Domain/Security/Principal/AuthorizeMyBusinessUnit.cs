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
            if (site == null || site.BusinessUnit == null)
            {
                return false;
            }

            return queryingPerson.BelongsToBusinessUnit(site.BusinessUnit,dateOnly);
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IBusinessUnit businessUnit)
        {
            if (businessUnit == null)
            {
                return false;
            }

            return queryingPerson.BelongsToBusinessUnit(businessUnit,dateOnly);
        }

    	public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPersonAuthorization authorization)
    	{
    		return queryingPerson.BelongsToBusinessUnit(authorization.BusinessUnitId, dateOnly);
    	}

	    public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeamAuthorization authorization)
	    {
			return queryingPerson.BelongsToBusinessUnit(authorization.BusinessUnitId, dateOnly);
		}

	    public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISiteAuthorization authorization)
	    {
			return queryingPerson.BelongsToBusinessUnit(authorization.BusinessUnitId, dateOnly);
		}
	}
}