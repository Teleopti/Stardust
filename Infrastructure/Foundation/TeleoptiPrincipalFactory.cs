using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class TeleoptiPrincipalFactory : IPrincipalFactory
	{
		private readonly IMakeOrganisationMembershipFromPerson _makeOrganisationMembershipFromPerson;
		private readonly IRetrievePersonNameForPerson _retrievePersonNameForPerson;

		public TeleoptiPrincipalFactory(
			IMakeOrganisationMembershipFromPerson makeOrganisationMembershipFromPerson,
			IRetrievePersonNameForPerson retrievePersonNameForPerson
			)
		{
			_makeOrganisationMembershipFromPerson = makeOrganisationMembershipFromPerson;
			_retrievePersonNameForPerson = retrievePersonNameForPerson;
		}
		
		public static TeleoptiPrincipalFactory Make()
		{
			var internalsFactory = new TeleoptiPrincipalInternalsFactory();
			return new TeleoptiPrincipalFactory(internalsFactory, internalsFactory);
		}
		
		public ITeleoptiPrincipal MakePrincipal(IPrincipalSource person, IDataSource dataSource, IBusinessUnit businessUnit, string tokenIdentity)
		{
			var identity = new TeleoptiIdentity(
				_retrievePersonNameForPerson.NameForPerson(person),
				dataSource, 
				businessUnit,
				WindowsIdentity.GetCurrent(),
				tokenIdentity
				);
			var principal = new TeleoptiPrincipal(identity, person);
			principal.Organisation = _makeOrganisationMembershipFromPerson.MakeOrganisationMembership(person);
			return principal;
		}

	}
}