using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public interface ICurrentPrincipalContext
	{
		void SetCurrentPrincipal(IPerson loggedOnUser, IDataSource dataSource, IBusinessUnit businessUnit, AuthenticationTypeOption teleoptiAuthenticationType);
		void SetCurrentPrincipal(ITeleoptiPrincipal principal);
	}
}