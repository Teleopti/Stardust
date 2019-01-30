using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class TeleoptiPrincipalForLegacyFactory : IPrincipalFactory
	{
		public ITeleoptiPrincipal MakePrincipal(IPrincipalSource source, IDataSource dataSource, string tokenIdentity)
		{
			var identity = new TeleoptiIdentityWithUnsafeBusinessUnit(
				source?.PrincipalName() ?? string.Empty,
				dataSource,
				source?.UnsafeBusinessUnit() as IBusinessUnit, 
				WindowsIdentity.GetCurrent(),
				tokenIdentity);
			return new TeleoptiPrincipalWithUnsafePerson(identity, source);
		}
	}
}