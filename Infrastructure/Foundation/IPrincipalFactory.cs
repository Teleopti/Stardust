using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public interface IPrincipalFactory
	{
		ITeleoptiPrincipal MakePrincipal(IPerson loggedOnUser, IDataSource dataSource, IBusinessUnit businessUnit, string tokenIdentity = null);
	}
}