using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public static class TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE
	{
		public static ITeleoptiPrincipal CurrentPrincipal => ServiceLocator_DONTUSE.CurrentTeleoptiPrincipal?.Current();
	}
}