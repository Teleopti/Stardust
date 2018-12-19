using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Logon
{
	public interface ICurrentPrincipalContext
	{
		void SetCurrentPrincipal(ITeleoptiPrincipal principal);
	}
}