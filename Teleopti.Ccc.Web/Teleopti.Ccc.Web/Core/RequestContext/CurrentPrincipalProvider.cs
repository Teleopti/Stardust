using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class CurrentPrincipalProvider : ICurrentPrincipalProvider
	{
		public TeleoptiPrincipal Current()
		{
			return TeleoptiPrincipal.Current;
		}
	}
}