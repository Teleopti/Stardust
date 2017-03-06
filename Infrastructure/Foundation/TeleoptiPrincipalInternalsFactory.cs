using System.Diagnostics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

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

		[DebuggerStepThrough]
		public virtual string NameForPerson(IPerson person)
		{
			try
			{
				return person?.Name.ToString() ?? string.Empty;
			}
			catch (NHibernate.ObjectNotFoundException exception)
			{
				throw new PersonNotFoundException("Person not found lazy loading the name", exception);
			}
		}
	}
}