using Teleopti.Interfaces.Domain;

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

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IBusinessUnit businessUnit)
		{
			return false;
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IAuthorizeOrganisationDetail authorizeOrganisationDetail)
		{
			return false;
		}
	}
}