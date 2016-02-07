using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Logon
{
	public interface ICurrentPrincipalContext
	{
		void SetCurrentPrincipal(IPerson loggedOnUser, IDataSource dataSource, IBusinessUnit businessUnit);
		void SetCurrentPrincipal(ITeleoptiPrincipal principal);
		void ResetPrincipal();
	}
}