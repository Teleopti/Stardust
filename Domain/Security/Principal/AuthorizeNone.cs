using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class AuthorizeNone : IAuthorizeAvailableData
	{
		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPerson person)
		{
			return false;
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
			return false;
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