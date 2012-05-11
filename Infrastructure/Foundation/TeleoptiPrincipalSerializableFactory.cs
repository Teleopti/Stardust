using System.Security.Principal;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class TeleoptiPrincipalSerializableFactory : IPrincipalFactory
	{
		private readonly IRegionalFactory _regionalFactory;
		private readonly IOrganisationMembershipFactory _organisationMembershipFactory;

		public TeleoptiPrincipalSerializableFactory(IRegionalFactory regionalFactory, IOrganisationMembershipFactory organisationMembershipFactory)
		{
			_regionalFactory = regionalFactory;
			_organisationMembershipFactory = organisationMembershipFactory;
		}

		public ITeleoptiPrincipal MakePrincipal(IPerson loggedOnUser, IDataSource dataSource, IBusinessUnit businessUnit, AuthenticationTypeOption teleoptiAuthenticationType)
		{
			var identity = new TeleoptiIdentity(loggedOnUser == null ? string.Empty : loggedOnUser.Name.ToString(), 
			                                    dataSource, businessUnit,
			                                    WindowsIdentity.GetCurrent(), 
			                                    teleoptiAuthenticationType
				);
			var principal = TeleoptiPrincipalSerializable.Make(identity, loggedOnUser);
			principal.Regional = _regionalFactory.MakeRegional(loggedOnUser);
			principal.Organisation = _organisationMembershipFactory.MakeOrganisationMembership(loggedOnUser);
			return principal;
		}
	}
}