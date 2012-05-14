using System.Security.Principal;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class TeleoptiPrincipalSerializableFactory : IPrincipalFactory
	{
		private readonly IMakeRegionalFromPerson _makeRegionalFromPerson;
		private readonly IMakeOrganisationMembershipFromPerson _makeOrganisationMembershipFromPerson;

		public TeleoptiPrincipalSerializableFactory(IMakeRegionalFromPerson makeRegionalFromPerson, IMakeOrganisationMembershipFromPerson makeOrganisationMembershipFromPerson)
		{
			_makeRegionalFromPerson = makeRegionalFromPerson;
			_makeOrganisationMembershipFromPerson = makeOrganisationMembershipFromPerson;
		}

		public ITeleoptiPrincipal MakePrincipal(IPerson loggedOnUser, IDataSource dataSource, IBusinessUnit businessUnit, AuthenticationTypeOption teleoptiAuthenticationType)
		{
			var identity = new TeleoptiIdentity(loggedOnUser.Id.Value.ToString(), 
			                                    dataSource, businessUnit,
			                                    WindowsIdentity.GetCurrent(), 
			                                    teleoptiAuthenticationType
				);
			var principal = TeleoptiPrincipalSerializable.Make(identity, loggedOnUser);
			principal.Regional = _makeRegionalFromPerson.MakeRegionalFromPerson(loggedOnUser);
			principal.Organisation = _makeOrganisationMembershipFromPerson.MakeOrganisationMembership(loggedOnUser);
			return principal;
		}
	}
}