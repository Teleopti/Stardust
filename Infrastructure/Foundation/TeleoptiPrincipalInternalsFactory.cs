using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class TeleoptiPrincipalInternalsFactory : IMakeRegionalFromPerson, IMakeOrganisationMembershipFromPerson, IRetrievePersonNameForPerson
	{
		public virtual IRegional MakeRegionalFromPerson(IPerson loggedOnUser)
		{
			return Regional.FromPerson(loggedOnUser);
		}

		public virtual IOrganisationMembership MakeOrganisationMembership(IPerson loggedOnUser)
		{
			return OrganisationMembership.FromPerson(loggedOnUser);
		}

		public virtual string NameForPerson(IPerson person)
		{
			return person == null ? string.Empty : person.Name.ToString();
		}
	}
}