using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class TeleoptiPrincipalInternalsFactory : IMakeRegionalFromPerson, IMakeOrganisationMembershipFromPerson
	{
		public IRegional MakeRegionalFromPerson(IPerson loggedOnUser)
		{
			return Regional.FromPerson(loggedOnUser);
		}

		public IOrganisationMembership MakeOrganisationMembership(IPerson loggedOnUser)
		{
			return OrganisationMembership.FromPerson(loggedOnUser);
		}

	}
}