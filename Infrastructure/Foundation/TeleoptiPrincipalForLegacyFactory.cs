using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class TeleoptiPrincipalForLegacyFactory : IPrincipalFactory
	{
		public ITeleoptiPrincipal MakePrincipal(IPrincipalSource person, IDataSource dataSource, IBusinessUnit businessUnit, string tokenIdentity)
		{
			var identity = new TeleoptiIdentityForLegacy(
				person?.PrincipalName() ?? string.Empty,
				dataSource,
				businessUnit,
				WindowsIdentity.GetCurrent(),
				tokenIdentity);
			return new TeleoptiPrincipalForLegacy(identity, person as IPerson);
		}
	}
}