using System.Diagnostics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class TeleoptiPrincipalInternalsFactory : IMakeRegionalFromPerson, IMakeOrganisationMembershipFromPerson, IRetrievePersonNameForPerson
	{
		public virtual IRegional MakeRegionalFromPerson(IPerson person)
		{
			var info = person.PermissionInformation;
			return new Regional(
				info.DefaultTimeZone(),
				info.CultureLCID() ?? 0,
				info.UICultureLCID() ?? 0);
		}

		public virtual IOrganisationMembership MakeOrganisationMembership(IPerson person)
		{
			return new OrganisationMembership().InitializeFromPerson(person);
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