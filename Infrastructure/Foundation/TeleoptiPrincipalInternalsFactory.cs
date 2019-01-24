using System.Diagnostics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class TeleoptiPrincipalInternalsFactory : IMakeOrganisationMembershipFromPerson
	{
		public virtual IOrganisationMembership MakeOrganisationMembership(IPrincipalSource person)
		{
			return new OrganisationMembership().Initialize(person);
		}

	}
}