using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Logon
{
	public interface IPrincipalFactory
	{
		ITeleoptiPrincipal MakePrincipal(IPerson person, IDataSource dataSource, IBusinessUnit businessUnit, string tokenIdentity);
	}
}