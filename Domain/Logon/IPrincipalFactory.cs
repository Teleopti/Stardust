using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Logon
{
	public interface IPrincipalFactory
	{
		ITeleoptiPrincipal MakePrincipal(IPerson person, IDataSource dataSource, IBusinessUnit businessUnit, string tokenIdentity);
	}
}