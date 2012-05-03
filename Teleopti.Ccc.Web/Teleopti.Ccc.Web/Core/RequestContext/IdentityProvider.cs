using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class IdentityProvider : IIdentityProvider
	{
		public TeleoptiIdentity Current()
		{
			return TeleoptiPrincipal.Current.Identity as TeleoptiIdentity;
		}
	}
}