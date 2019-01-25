using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class AuthorizeEveryone : IAuthorizeAvailableData
	{
		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPerson person)
		{
			return true;
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeam team)
		{
			return true;
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISite site)
		{
			return true;
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, Guid businessUnitId)
		{
			return true;
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPersonAuthorization authorization)
		{
			return true;
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeamAuthorization authorization)
		{
			return true;
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISiteAuthorization authorization)
		{
			return true;
		}
	}
}