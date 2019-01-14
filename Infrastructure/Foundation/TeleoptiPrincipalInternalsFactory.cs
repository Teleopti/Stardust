using System.Diagnostics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class TeleoptiPrincipalInternalsFactory : IMakeRegionalFromPerson, IMakeOrganisationMembershipFromPerson, IRetrievePersonNameForPerson
	{
		public virtual IRegional MakeRegionalFromPerson(IPrincipalSource person)
		{
			return new Regional(
				person.PrincipalTimeZone(),
				person.PrincipalCultureLCID() ?? 0,
				person.PrincipalUICultureLCID() ?? 0);
		}

		public virtual IOrganisationMembership MakeOrganisationMembership(IPrincipalSource person)
		{
			return new OrganisationMembership().Initialize(person);
		}

		[DebuggerStepThrough]
		public virtual string NameForPerson(IPrincipalSource person)
		{
			try
			{
				return person?.PrincipalName() ?? string.Empty;
			}
			catch (NHibernate.ObjectNotFoundException exception)
			{
				throw new PersonNotFoundException("Person not found lazy loading the name", exception);
			}
		}
	}
}