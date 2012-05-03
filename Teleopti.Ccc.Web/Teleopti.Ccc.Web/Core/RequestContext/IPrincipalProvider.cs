using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	/// <summary>
	/// Returns Existing/Current Teleopti Principal from Thread
	/// </summary>
	public interface IPrincipalProvider
	{
		ITeleoptiPrincipal Current();
	}
}