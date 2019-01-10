using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class TeleoptiPrincipalFactory : IPrincipalFactory
	{
		public ITeleoptiPrincipal MakePrincipal(IPerson person, IDataSource dataSource, IBusinessUnit businessUnit, string tokenIdentity)
		{
			var identity = new TeleoptiIdentity(
				person?.Name.ToString() ?? string.Empty,
				dataSource,
				businessUnit,
				WindowsIdentity.GetCurrent(),
				tokenIdentity);
			return new TeleoptiPrincipal(identity, person);
		}
	}
}