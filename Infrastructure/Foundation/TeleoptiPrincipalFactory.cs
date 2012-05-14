using System.Security.Principal;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class TeleoptiPrincipalFactory : IPrincipalFactory
	{
		public ITeleoptiPrincipal MakePrincipal(IPerson loggedOnUser, IDataSource dataSource, IBusinessUnit businessUnit, AuthenticationTypeOption teleoptiAuthenticationType)
		{
			var identity = new TeleoptiIdentity(loggedOnUser == null ? string.Empty : loggedOnUser.Name.ToString(), dataSource, businessUnit,
			                                    WindowsIdentity.GetCurrent(), teleoptiAuthenticationType);
			var principal = new TeleoptiPrincipal(identity, loggedOnUser);
			return principal;
		}

	}
}