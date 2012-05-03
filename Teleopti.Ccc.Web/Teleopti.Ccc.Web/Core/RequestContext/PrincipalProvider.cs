using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class PrincipalProvider : IPrincipalProvider
	{
		public TeleoptiPrincipal Current()
		{
			return TeleoptiPrincipal.Current;
		}
	}
}