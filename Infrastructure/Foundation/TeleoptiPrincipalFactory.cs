using System.Security.Principal;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class TeleoptiPrincipalFactory : IPrincipalFactory
	{
		public ITeleoptiPrincipal MakePrincipal(IPerson person, IDataSource dataSource, IBusinessUnit businessUnit, string tokenIdentity)
		{
			var identity = new TeleoptiIdentity(
				person == null ? string.Empty : person.Name.ToString(),
				dataSource,
				businessUnit,
				WindowsIdentity.GetCurrent(),
				tokenIdentity);
			return new TeleoptiPrincipal(identity, person);
		}

	}
}