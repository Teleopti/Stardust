using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public interface IOrganisationMembershipFactory
	{
		IOrganisationMembership MakeOrganisationMembership(IPerson loggedOnUser);
	}
}