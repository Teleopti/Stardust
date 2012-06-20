using System;
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
			try
			{
				return person == null ? string.Empty : person.Name.ToString();
			}
			catch (NHibernate.ObjectNotFoundException exception)
			{
				throw new PersonNotFoundException("Person not found", exception);
			}
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors"), 
	System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class PersonNotFoundException : Exception
	{
		public PersonNotFoundException(string message, Exception innerException) : base(message, innerException) { }
	}
}