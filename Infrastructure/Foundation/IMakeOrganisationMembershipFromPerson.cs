using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public interface IMakeOrganisationMembershipFromPerson
	{
		IOrganisationMembership MakeOrganisationMembership(IPerson loggedOnUser);
	}
}