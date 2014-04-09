using System.Security.Principal;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class TeleoptiPrincipalCacheableFactory : IPrincipalFactory
	{
		private readonly IMakeRegionalFromPerson _makeRegionalFromPerson;
		private readonly IMakeOrganisationMembershipFromPerson _makeOrganisationMembershipFromPerson;
		private readonly IRetrievePersonNameForPerson _retrievePersonNameForPerson;

		public TeleoptiPrincipalCacheableFactory(
			IMakeRegionalFromPerson makeRegionalFromPerson, 
			IMakeOrganisationMembershipFromPerson makeOrganisationMembershipFromPerson,
			IRetrievePersonNameForPerson retrievePersonNameForPerson
			)
		{
			_makeRegionalFromPerson = makeRegionalFromPerson;
			_makeOrganisationMembershipFromPerson = makeOrganisationMembershipFromPerson;
			_retrievePersonNameForPerson = retrievePersonNameForPerson;
		}

		public ITeleoptiPrincipal MakePrincipal(IPerson loggedOnUser, IDataSource dataSource, IBusinessUnit businessUnit, string tokenIdentity = null)
		{
			var identity = new TeleoptiIdentity(_retrievePersonNameForPerson.NameForPerson(loggedOnUser), 
			                                    dataSource, businessUnit,
			                                    WindowsIdentity.GetCurrent(), tokenIdentity
				);
			var principal = TeleoptiPrincipalCacheable.Make(identity, loggedOnUser);
			principal.Regional = _makeRegionalFromPerson.MakeRegionalFromPerson(loggedOnUser);
			principal.Organisation = _makeOrganisationMembershipFromPerson.MakeOrganisationMembership(loggedOnUser);
			return principal;
		}
	}
}