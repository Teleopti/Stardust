using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class AuthorizeMyOwn : IAuthorizeAvailableData
    {
        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPerson person)
        {
            return queryingPerson.IsUser(person);
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeam team)
        {
            return false;
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISite site)
        {
            return false;
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, Guid businessUnitId)
        {
            return false;
        }

    	public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPersonAuthorization authorization)
    	{
    		return queryingPerson.IsUser(authorization.PersonId);
    	}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeamAuthorization authorization)
		{
			return false;
		}

	    public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISiteAuthorization authorization)
	    {
			return false;
		}
	}
}