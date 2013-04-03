using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Common
{
	public class CurrentIdentity : ICurrentIdentity
	{
		public ITeleoptiIdentity Current()
		{
			return TeleoptiPrincipal.Current.Identity as ITeleoptiIdentity;
		}
	}
}