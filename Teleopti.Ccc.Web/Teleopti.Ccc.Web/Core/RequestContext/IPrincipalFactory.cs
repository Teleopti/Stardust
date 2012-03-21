using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	/// <summary>
	/// Will give correct principal object back
	/// </summary>
	public interface IPrincipalFactory
	{
		TeleoptiPrincipal Generate();
	}
}